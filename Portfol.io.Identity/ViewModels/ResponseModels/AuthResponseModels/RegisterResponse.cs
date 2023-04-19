namespace Portfol.io.Identity.ViewModels.ResponseModels.AuthResponseModels
{
    /// <summary>
    /// Класс ответа из регистрации
    /// </summary>
    public class RegisterResponse
    {
        /// <summary>
        /// Получить/установить id зарегистрированного пользователя
        /// </summary>
        public string? userId { get; set; }
        /// <summary>
        /// Получить/установить ссылку возврата
        /// </summary>
        public string? returnUrl { get; set; }
        /// <summary>
        /// Получить/установить токен доступа
        /// </summary>
        public string? access_token { get; set; }
        /// <summary>
        /// Получить/установить время жизни токена доступа
        /// </summary>
        public DateTime? expires { get; set; }
    }
}
