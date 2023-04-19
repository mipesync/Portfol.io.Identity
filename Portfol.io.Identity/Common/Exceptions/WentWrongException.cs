namespace Portfol.io.Identity.Common.Exceptions
{
    /// <summary>
    /// Класс ошибки "Что-то пошло не так"
    /// </summary>
    public class WentWrongException : Exception
    {
        /// <summary>
        /// Конструктор с начальным текстом ошибки
        /// </summary>
        public WentWrongException() : base("Что-то пошло не так") { }

        /// <summary>
        /// Инициализация начальных параметров
        /// </summary>
        /// <param name="message">Текст сообщения</param>
        public WentWrongException(string message) : base(message) { }
    }
}
