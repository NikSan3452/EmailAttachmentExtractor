﻿#region *   License     *

/*
    SimpleHelpers - FileEncoding

    Copyright © 2014 Khalid Salomão

    Permission is hereby granted, free of charge, to any person
    obtaining a copy of this software and associated documentation
    files (the “Software”), to deal in the Software without
    restriction, including without limitation the rights to use,
    copy, modify, merge, publish, distribute, sublicense, and/or sell
    copies of the Software, and to permit persons to whom the
    Software is furnished to do so, subject to the following
    conditions:

    The above copyright notice and this permission notice shall be
    included in all copies or substantial portions of the Software.

    THE SOFTWARE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND,
    EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
    OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
    NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
    HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
    WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
    FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
    OTHER DEALINGS IN THE SOFTWARE.

    License: http://www.opensource.org/licenses/mit-license.php
    Website: https://github.com/khalidsalomao/SimpleHelpers.Net
 */

#endregion

using System.IO;
using System.Text;
using Ude;

namespace EmailAttachmentExtractor.Helpers;

public class FileEncoding : ITextEncoder
{
    private const int DEFAULT_BUFFER_SIZE = 128 * 1024;

    private readonly List<string> singleEncodings = new();

    private readonly CharsetDetector ude = new();
    private bool _started;


    /// <summary>
    ///     If the detection has reached a decision.
    /// </summary>
    /// <value>The done.</value>
    public bool Done { get; set; }

    /// <summary>
    ///     Detected encoding name.
    /// </summary>
    public string? EncodingName { get; set; }

    /// <summary>
    ///     If the data contains textual data.
    /// </summary>
    public bool IsText { get; set; }

    /// <summary>
    ///     If the file or data has any mark indicating encoding information (byte order mark).
    /// </summary>
    public bool HasByteOrderMark { get; set; }

    /// <summary>
    ///     Decodes a given string from Windows-1251 encoding to a UTF-8 string. If the decoding fails, it attempts to detect
    ///     the encoding and decode accordingly.
    /// </summary>
    /// <param name="text">The input string to be decoded.</param>
    /// <returns>The decoded string.</returns>
    /// <remarks>
    ///     This method first tries to decode the input string assuming it is encoded in Windows-1251. If an exception occurs
    ///     during this process, it falls back to detecting the encoding using the `DetectFileEncoding` method and then decodes
    ///     the string with the detected encoding.
    /// </remarks>
    public string Decode(string text)
    {
        byte[] bytes;
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        try
        {
            bytes = Encoding.GetEncoding(1251).GetBytes(text);
            var decodedText = Encoding.GetEncoding(1251).GetString(bytes);

            return decodedText;
        }
        catch
        {
            // ignored
        }

        bytes = Encoding.Default.GetBytes(text);
        var detectedEncoding = DetectFileEncoding(new MemoryStream(bytes));
        return detectedEncoding.GetString(bytes);
    }

    /// <summary>
    ///     Tries to detect the file encoding.
    /// </summary>
    /// <param name="inputFilename">The input filename.</param>
    /// <param name="defaultIfNotDetected">The default encoding if none was detected.</param>
    /// <returns></returns>
    public static Encoding DetectFileEncoding(string inputFilename, Encoding defaultIfNotDetected = null!)
    {
        using (var stream = new FileStream(inputFilename, FileMode.Open, FileAccess.Read,
                   FileShare.ReadWrite | FileShare.Delete, DEFAULT_BUFFER_SIZE))
        {
            return DetectFileEncoding(stream) ?? defaultIfNotDetected;
        }
    }

    /// <summary>
    ///     Tries to detect the file encoding.
    /// </summary>
    /// <param name="inputStream">The input stream.</param>
    /// <param name="defaultIfNotDetected">The default encoding if none was detected.</param>
    /// <returns></returns>
    public static Encoding DetectFileEncoding(Stream inputStream, Encoding defaultIfNotDetected = null!)
    {
        var det = new FileEncoding();
        det.Detect(inputStream);
        return det.Complete() ?? defaultIfNotDetected;
    }

    /// <summary>
    ///     Tries to detect the file encoding.
    /// </summary>
    /// <param name="inputData">The input data.</param>
    /// <param name="start">The start.</param>
    /// <param name="count">The count.</param>
    /// <param name="defaultIfNotDetected">The default encoding if none was detected.</param>
    /// <returns></returns>
    public static Encoding DetectFileEncoding(byte[] inputData, int start, int count,
        Encoding defaultIfNotDetected = null!)
    {
        var det = new FileEncoding();
        det.Detect(inputData, start, count);
        return det.Complete() ?? defaultIfNotDetected;
    }

    /// <summary>
    ///     Tries to load file content with the correct encoding.
    /// </summary>
    /// <param name="filename">The filename.</param>
    /// <param name="defaultValue">The default value if unable to load file content.</param>
    /// <returns>File content</returns>
    public static string TryLoadFile(string filename, string defaultValue = "")
    {
        try
        {
            if (File.Exists(filename))
            {
                // enable file encoding detection
                var encoding = DetectFileEncoding(filename);
                // Load data based on parameters
                return File.ReadAllText(filename, encoding);
            }
        }
        catch
        {
        }

        return defaultValue;
    }

    /// <summary>
    ///     Detects if contains textual data.
    /// </summary>
    /// <param name="rawData">The raw data.</param>
    public static bool CheckForTextualData(byte[] rawData)
    {
        return CheckForTextualData(rawData, 0, rawData.Length);
    }

    /// <summary>
    ///     Detects if contains textual data.
    /// </summary>
    /// <param name="rawData">The raw data.</param>
    /// <param name="start">The start.</param>
    /// <param name="count">The count.</param>
    public static bool CheckForTextualData(byte[] rawData, int start, int count)
    {
        if (rawData.Length < count || count < 4 || start + 1 >= count)
            return true;

        if (CheckForByteOrderMark(rawData, start)) return true;

        // http://stackoverflow.com/questions/910873/how-can-i-determine-if-a-file-is-binary-or-text-in-c
        // http://www.gnu.org/software/diffutils/manual/html_node/Binary.html
        // count the number od null bytes sequences
        // considering only sequeces of 2 0s: "\0\0" or control characters below 10
        var nullSequences = 0;
        var controlSequences = 0;
        for (var i = start + 1; i < count; i++)
            if (rawData[i - 1] == 0 && rawData[i] == 0)
            {
                if (++nullSequences > 1)
                    break;
            }
            else if (rawData[i - 1] == 0 && rawData[i] < 10)
            {
                ++controlSequences;
            }

        // is text if there is no null byte sequences or less than 10% of the buffer has control caracteres
        return nullSequences == 0 && controlSequences <= rawData.Length / 10;
    }

    /// <summary>
    ///     Detects if data has bytes order mark to indicate its encoding for textual data.
    /// </summary>
    /// <param name="rawData">The raw data.</param>
    /// <param name="start">The start.</param>
    /// <returns></returns>
    private static bool CheckForByteOrderMark(byte[] rawData, int start = 0)
    {
        if (rawData.Length - start < 4)
            return false;
        // Detect encoding correctly (from Rick Strahl's blog)
        // http://www.west-wind.com/weblog/posts/2007/Nov/28/Detecting-Text-Encoding-for-StreamReader
        if (rawData[start] == 0xef && rawData[start + 1] == 0xbb && rawData[start + 2] == 0xbf)
            // Encoding.UTF8;
            return true;
        if (rawData[start] == 0xfe && rawData[start + 1] == 0xff)
            // Encoding.Unicode;
            return true;
        if (rawData[start] == 0 && rawData[start + 1] == 0 && rawData[start + 2] == 0xfe && rawData[start + 3] == 0xff)
            // Encoding.UTF32;
            return true;
        if (rawData[start] == 0x2b && rawData[start + 1] == 0x2f && rawData[start + 2] == 0x76)
            // Encoding.UTF7;
            return true;
        return false;
    }

    /// <summary>
    ///     Resets this instance.
    /// </summary>
    public void Reset()
    {
        _started = false;
        Done = false;
        HasByteOrderMark = false;
        singleEncodings.Clear();
        ude.Reset();
        EncodingName = null!;
    }

    /// <summary>
    ///     Detects the encoding of textual data of the specified input data.
    /// </summary>
    /// <param name="inputData">The input data.</param>
    /// <returns>Detected encoding name</returns>
    public string? Detect(Stream inputData)
    {
        const int bufferSize = 16 * 1024;
        const int maxIterations = 20 * 1024 * 1024 / bufferSize;
        var i = 0;
        var buffer = new byte[bufferSize];
        while (i++ < maxIterations)
        {
            var sz = inputData.Read(buffer, 0, buffer.Length);
            if (sz <= 0) break;
            Detect(buffer, 0, sz);
            if (Done)
                break;
        }

        Complete();
        return EncodingName;
    }

    /// <summary>
    ///     Detects the encoding of textual data of the specified input data.
    /// </summary>
    /// <param name="inputData">The input data.</param>
    /// <param name="start">The start.</param>
    /// <param name="count">The count.</param>
    /// <returns>Detected encoding name</returns>
    public string? Detect(byte[] inputData, int start, int count)
    {
        if (Done)
            return EncodingName;
        if (!_started)
        {
            Reset();
            _started = true;
            if (!CheckForTextualData(inputData, start, count))
            {
                IsText = false;
                Done = true;
                return EncodingName;
            }

            HasByteOrderMark = CheckForByteOrderMark(inputData, start);
            IsText = true;
        }

        // execute charset detector                
        ude.Feed(inputData, start, count);
        ude.DataEnd();
        if (ude.IsDone() && !string.IsNullOrEmpty(ude.Charset))
        {
            Done = true;
            return EncodingName;
        }

        const int bufferSize = 4 * 1024;

        // singular buffer detection
        if (singleEncodings.Count < 2000)
        {
            var u = new CharsetDetector();
            var step = count - start < bufferSize ? count - start : bufferSize;
            for (var i = start; i < count; i += step)
            {
                u.Reset();
                if (i + step > count)
                    u.Feed(inputData, i, count - i);
                else
                    u.Feed(inputData, i, step);
                u.DataEnd();
                if (u.Confidence > 0.3 && !string.IsNullOrEmpty(u.Charset))
                    singleEncodings.Add(u.Charset);
            }
        }

        return EncodingName;
    }

    /// <summary>
    ///     Finalize detection phase and gets detected encoding name.
    /// </summary>
    /// <returns></returns>
    public Encoding Complete()
    {
        Done = true;
        ude.DataEnd();
        if (ude.IsDone() && !string.IsNullOrEmpty(ude.Charset))
            EncodingName = ude.Charset;
        else if (singleEncodings.Count > 0)
            // vote for best encoding
            EncodingName = singleEncodings.GroupBy(i => i)
                .OrderByDescending(i => i.Count() *
                                        (i.Key.StartsWith("UTF-32") ? 2 :
                                            i.Key.StartsWith("UTF-16") ? 1.8 :
                                            i.Key.StartsWith("UTF-8") ? 1.5 :
                                            i.Key.StartsWith("UTF-7") ? 1.3 :
                                            i.Key != "ASCII" ? 1 : 0.2))
                .Select(i => i.Key).FirstOrDefault();
        if (!string.IsNullOrEmpty(EncodingName))
            return Encoding.GetEncoding(EncodingName);
        return null!;
    }
}