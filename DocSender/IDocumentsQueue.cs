namespace DocSender
{
    /// <summary>
    /// Представляет очередь документов на отправку внешней системе.
    /// </summary>
    public interface IDocumentsQueue
    {
        /// <summary>
        /// Ставит документ в очередь на отправку.
        /// </summary>
        /// <param name="document">
        /// Документ, который нужно отправить.
        /// </param>
        void Enqueue(Document document);
    }
}