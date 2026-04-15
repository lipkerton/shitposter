namespace listener.Domain.Constants;

public static class AppConstants
{

    public const string httpClientName = "SOAPClient";
    public const string httpContentType = "text/xml";

    public const string logRequestMessageInfo = "Запрос на {1}...";
    public const string logValidationError = "Пропущена новость. Ошибки в данных: {1}";
    public const string logStatusCodeError = "Не удалось выполнить запрос к эндпоинту {1}. Статус код: {2}";
    public const string logEmptyNewsWarning = "Нет новостей за указанный период.";
    public const string logRequestStartInfo = "Начаты запросы за новостями...";
    public const string logAuthInfo = "Аутентификация...";
    public const string logAuthSuccessInfo = "Аутентификация прошла успешно!";
    public const string logAuthFailureInfo = "Аутентификация прошла плохо!";

    /* 
    константы описывают пространства имен для парсинга ответов API
    (чтобы найти тег в таком пространтсве имен его нужно объединить с наименованием пространства имен).
    */
    public const string xmlNamespace = "http://schemas.xmlsoap.org/soap/envelope/";
    public const string apiNamespace = "http://ifx.ru/IFX3WebService";
    
    // константы описывают теги для парсинга ответов API.
    public const string xmlBodyTag = "Body";
        
        // GetRealtimeNewsByProduct
        public const string responseXmlGrnmrespTag = "grnmresp";
        public const string responseXmlMbnlTag = "mbnl";
        public const string responseXmlC_nwliTag = "c_nwli";
        public const string responseXmlIndexTag = "i";
        public const string responseXmlHeaderTag = "h";
        public const string responseXmlPubDateTag = "pd";
    
        // GetEntireNewsById
        public const string requestXmlMbnidTag = "mbnid";
        public const string requestXmlGenmreqTag = "genmreq";
        public const string responseXmlGenmrespTag = "genmresp";
        public const string responseXmlMbnTag = "mbn";
        public const string responseXmlContentTag = "c";
}