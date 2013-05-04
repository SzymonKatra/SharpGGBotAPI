
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
}
