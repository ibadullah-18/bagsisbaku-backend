using bagsisbaku.Application.Common.Results;

namespace bagsisbaku.Application.Announcements;

public static class EmailAnnouncementErrors
{
    public static readonly Error AuthenticationRequired =
        Error.Unauthorized(
            "email-announcements.authentication-required",
            "Email elanı göndərmək üçün sistemə daxil olmaq lazımdır.");

    public static readonly Error AdministratorRequired =
        Error.Forbidden(
            "email-announcements.administrator-required",
            "Bu əməliyyatı yerinə yetirmək üçün admin icazəsi lazımdır.");

    public static readonly Error SubjectRequired =
        Error.Validation(
            "email-announcements.subject-required",
            "Email başlığı boş ola bilməz.");

    public static readonly Error HtmlBodyRequired =
        Error.Validation(
            "email-announcements.html-body-required",
            "Email məzmunu boş ola bilməz.");

    public static readonly Error TextBodyRequired =
        Error.Validation(
            "email-announcements.text-body-required",
            "Email mətn versiyası boş ola bilməz.");

    public static readonly Error RecipientRequired =
        Error.Validation(
            "email-announcements.recipient-required",
            "Ən azı bir email alıcısı seçilməlidir.");

    public static readonly Error InvalidEmail =
        Error.Validation(
            "email-announcements.invalid-email",
            "Alıcı email ünvanlarından biri düzgün deyil.");

    public static readonly Error CustomerNotFound =
        Error.NotFound(
            "email-announcements.customer-not-found",
            "Seçilmiş müştərilərdən biri tapılmadı.");

    public static readonly Error NotFound =
        Error.NotFound(
            "email-announcements.not-found",
            "Email elanı tapılmadı.");

    public static readonly Error NoFailedRecipients =
        Error.Conflict(
            "email-announcements.no-failed-recipients",
            "Yenidən göndəriləcək uğursuz alıcı yoxdur.");

    public static readonly Error AlreadyProcessing =
        Error.Conflict(
            "email-announcements.already-processing",
            "Email elanı artıq göndərilmə mərhələsindədir.");

    public static readonly Error ProcessingFailed =
        Error.Failure(
            "email-announcements.processing-failed",
            "Email elanı işlənərkən gözlənilməyən xəta baş verdi.");
}