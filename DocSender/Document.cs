namespace DocSender
{
    /// <summary>
    /// Определяет модель документа.
    /// </summary>
    public sealed class Document
    {
        /// <summary>
        /// Возвращает или задает идентификатор документа.
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// Возвращает или задает идентификатор пакета документов.
        /// </summary>
        /// <remarks>
        /// Документы могут быть объединены в пакет.
        /// Если это свойство равно <see langword="null"/>, то документ существует вне пакета,
        /// сам по себе.
        /// </remarks>
        public int? PackageId { get; set; }
        
        /// <summary>
        /// Возвращает или задает название документа.
        /// </summary>
        public string Title { get; set; }
        
        /// <summary>
        /// Возвращает или задает тип документа.
        /// </summary>
        public string DocumentType { get; set; }
    }}