using System;

namespace SharpGGBotAPI
{
    /// <summary>
    /// Dostępne statusy GG.
    /// </summary>
    public enum Status
    {
        /// <summary>
        /// Brak. Oznacza zostawienie takiego samego statusu.
        /// </summary>
        None,
        /// <summary>
        /// Dostępny.
        /// </summary>
        Available,
        /// <summary>
        /// Zaraz wracam.
        /// </summary>
        Busy,
        /// <summary>
        /// Niewidoczny.
        /// </summary>
        Invisible,
        /// <summary>
        /// Nie przeszkadzać.
        /// </summary>
        DoNotDisturb,
        /// <summary>
        /// PoGGadaj ze mną.
        /// </summary>
        FreeForCall,
        /// <summary>
        /// Reklamowy.
        /// </summary>
        Advertising,
    }

    /// <summary>
    /// Rodzaje operacji PUSH.
    /// </summary>
    public enum PushOperation
    {
        /// <summary>Brak.</summary>
        None = 0,
        /// <summary>Zmiana statusu.</summary>
        SetStatus = 1,
        /// <summary>Wysłanie wiadomości.</summary>
        SendMessage = 2,
        /// <summary>Wysłanie obrazka na serwer.</summary>
        ImageSend = 3,
        /// <summary>Sprawdzenie czy obrazek istnieje na serwerze.</summary>
        ImageExists = 4,
        /// <summary>Pobranie obrazka z serwera.</summary>
        ImageDownload = 5,
        /// <summary>Sprawdzenie czy dany numer jest botem.</summary>
        IsBot = 6,
    }

    /// <summary>
    /// Kody błędów wiadomości PUSH.
    /// </summary>
    public enum PushErrorCode
    {
        /// <summary>Nieznany błąd.</summary>
        Unknown = -1,
        /// <summary>Sukces.</summary>
        Success = 0,
        /// <summary>Nieprawidłowy numer bota.</summary>
        BadBotUin = 1,
        /// <summary>Bot niezarejestrowany lub nieaktywny na tej maszynie.</summary>
        BotUnregisteredOrNotActive = 2,
        /// <summary>Brak wiadomości do wysłania.</summary>
        NoMessage = 3,
        /// <summary>Błędny lub nieważny token.</summary>
        BadToken = 4,
        /// <summary>Liczba odbiorców wiadomości nie może być większa od 1000.</summary>
        NumberOfRecipientsExceeded1000 = 5,
        /// <summary>Brak poprawnych numerów odbiorców na liście.</summary>
        BadRecipientsUins = 6,
        /// <summary>Długość opisu nie może przekraczać 255 bajtów.</summary>
        DescriptionWasLongerThan255 = 7,
        /// <summary>Wartość statusu musi być prawidłową liczbą.</summary>
        BadStatusCode = 8,
        /// <summary>Niepoprawny zasób. Dostępne zasoby: setStatus, sendMessage.</summary>
        BadResource = 9,
        /// <summary>Za mało parametrów żądania, musi zawierać przynajmniej to oraz msg.</summary>
        NotEnoughRequestParametrsShouldBeToMsg = 10,
        /// <summary>Za mało parametrów żądania, musi zawierać: status oraz opcjonalnie desc.</summary>
        NotEnoughRequestParametrsShouldStatusDescription = 11,
        /// <summary>Niepoprawne żądanie. Upewnij się, że parametry żądania są w kodowaniu procentowym (Content-Type: application/x-www-form-urlencoded).</summary>
        BadRequest = 12,
        /// <summary>Nie udało się wysłać wiadomości.</summary>
        FailedToSendMessages = 13,
        /// <summary>Nie udało się ustawić statusu.</summary>
        FailedToSetStatus = 14,
        /// <summary>Nieprawidłowy nagłówek Content-Length.</summary>
        BadContentLengthHeader = 15,
        /// <summary>Długość żądania przekracza dopuszczalny rozmiar.</summary>
        RequestTooLong = 16,
        /// <summary>Długość nagłówka przekracza dopuszczalny rozmiar.</summary>
        RequestHeaderTooLong = 17,
        /// <summary>Nie znalazłem obrazka ani w wiadomości protokołowej, ani w pamięci podręcznej.</summary>
        ImageNotFound = 18,
        /// <summary>Długość wiadomości HTML jest niezgodna z informacją z nagłówka.</summary>
        HtmlMessageLengthIncompatibleWithHeaderInformation = 19,
        /// <summary>Długość wiadomości tekstowej jest niezgodna z informacją z nagłówka.</summary>
        PlainMessageLengthIncompatibleWithHeaderInformation = 20,
        /// <summary>Któreś z pól struktury (tekst, html, formatowanie lub obrazek) przekraczają dopuszczalną długość.</summary>
        AnyOfTheStructureFieldsAreTooLong = 21,
    }

    /// <summary>
    /// Flagi formatowania tekstu.
    /// </summary>
    [Flags]
    public enum MessageFormatting
    {
        /// <summary>Brak formatowania.</summary>
        None = 0,
        /// <summary>Tekst pogrubiony.</summary>
        Bold = 1 << 0,
        /// <summary>Tekst pochyły.</summary>
        Italic = 1 << 1,
        /// <summary>Tekst podkreślony.</summary>
        Underline = 1 << 2,
        /// <summary>Tekst skreślony.</summary>
        Erasure = 1 << 3,
        /// <summary>Dodaj nową linię na koniec tekstu.</summary>
        NewLine = 1 << 4,
        /// <summary>Indeks górny.</summary>
        Superscript = 1 << 5,
        /// <summary>Indeks dolny.</summary>
        Subscript = 1 << 6,
    }
}
