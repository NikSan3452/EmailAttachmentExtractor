using System.IO;
using System.Text;
using EmailAttachmentExtractor.Helpers.Interfaces;
using Ude;

namespace EmailAttachmentExtractor.Helpers;

/// <summary>
///     Класс для работы с кодировками.
///     Наследуется от интерфейса <see cref="ITextEncoder" />
/// </summary>
public class TextEncoder : ITextEncoder
{
    /// <summary>
    ///     Кодирует текст, определяя его кодировку и преобразуя его в нужный формат.
    /// </summary>
    /// <remarks>
    ///     Метод выполняет следующие действия:
    ///     <list type="number">
    ///         <item>Регистрирует провайдер для поддержки дополнительных кодировок.</item>
    ///         <item>По умолчанию пытается закодировать текст в кодировке Windows-1251.</item>
    ///         <item>Если кодирование в Windows-1251 не удается, определяет кодировку текста с помощью библиотеки NCharDet.</item>
    ///         <item>Возвращает декодированный текст в соответствии с определенной кодировкой.</item>
    ///     </list>
    /// </remarks>
    /// <seealso cref="DetectEncoding" />
    public string Encode(string text)
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
        var detectedEncoding = DetectEncoding(new MemoryStream(bytes));
        return detectedEncoding.GetString(bytes);
    }

    /// <summary>
    ///     Определяет кодировку текста с помощью библиотеки NCharDet.
    /// </summary>
    /// <remarks>
    ///     Метод выполняет следующие действия:
    ///     <list type="number">
    ///         <item>Устанавливает позицию потока в начало.</item>
    ///         <item>Использует CharsetDetector для определения кодировки текста.</item>
    ///         <item>
    ///             Возвращает объект Encoding, соответствующий определенной кодировке, или кодировку по умолчанию, если
    ///             определение не удалось.
    ///         </item>
    ///     </list>
    /// </remarks>
    private static Encoding DetectEncoding(MemoryStream stream)
    {
        stream.Position = 0;

        var detector = new CharsetDetector();
        detector.Feed(stream);
        detector.DataEnd();
        return detector.Charset != null ? Encoding.GetEncoding(detector.Charset) : Encoding.Default;
    }
}