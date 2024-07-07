namespace EmailAttachmentExtractor.Helpers.Interfaces;

/// <summary>
///     Интерфейс для работы с кодировками.
/// </summary>
public interface ITextEncoder
{
    /// <summary>
    ///     Кодирует текст, определяя его кодировку и преобразуя его в нужный формат.
    /// </summary>
    string Encode(string text);
}