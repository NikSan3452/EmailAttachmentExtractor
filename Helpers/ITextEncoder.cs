namespace EmailAttachmentExtractor.Helpers;

public interface ITextEncoder
{
    string Decode(string text);
}