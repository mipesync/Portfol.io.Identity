using Portfol.io.Identity.DTO;
using Portfol.io.Identity.DTO.ResponseModels.AuthResponseModels;

namespace Portfol.io.Identity.Repositories
{
    /// <summary>
    /// Интерфейс репозитория авторизации
    /// </summary>
    public interface IAuthRepository
    {
        /// <summary>
        /// Метод авторизации
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<LoginResponse> Login(LoginDto model);

        /// <summary>
        /// Метод регистрации
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<RegisterResponse> Register(RegisterDto model);

        /// <summary>
        /// Метод подтверждения смены пароля
        /// </summary>
        /// <param name="email">Почта от аккаунта, для которой надо сбросить пароль</param>
        /// <param name="hostUrl">Адрес хоста фронтенда</param>
        /// <returns></returns>
        Task ForgotPassword(string email, string hostUrl);

        /// <summary>
        /// Метод смены пароля
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task ResetPassword(ResetPasswordDto model);

        /// <summary>
        /// Метод отправки письма подтверждения почты
        /// </summary>
        /// <param name="userId">Id пользователя, которому нужно подтверждение</param>
        /// <param name="code">Код подтверждения из письма</param>
        /// <param name="returnUrl">Адрес возврата</param>
        /// <returns></returns>
        Task<ConfirmEmailResponse> ConfirmEmail(string userId, string code, string? returnUrl);

        /// <summary>
        /// Метод повторной отправки письма подтверждения почты
        /// </summary>
        /// <param name="email">Почта от аккаунта, для которой надо сбросить пароль</param>
        /// <param name="returnUrl">Адрес возврата</param>
        /// <param name="hostUrl">Адрес хоста фронтенда</param>
        /// <returns></returns>
        Task ResendEmailConfirmation(string email, string? returnUrl, string hostUrl);

        /// <summary>
        /// Метод обновления токена обновления
        /// </summary>
        /// <param name="refresh">Старый токен обновления</param>
        /// <returns></returns>
        Task<RefreshTokenResponse> RefreshToken(string refresh);

        /// <summary>
        /// Метод отзыва токена обновления
        /// </summary>
        /// <param name="userId">Id пользователя, которому нужно отозвать токен</param>
        /// <returns></returns>
        Task Revoke(string userId);
    }
}
