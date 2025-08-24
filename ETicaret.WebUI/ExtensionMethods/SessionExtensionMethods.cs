using Newtonsoft.Json;//Json'a dönüştürme işlemlerini yapan kütüphane

namespace ETicaret.WebUI.ExtensionMethods
{
    public static class SessionExtensionMethods
    {
        public static void SetJson(this ISession session,string key, object value) //Bu metod, bir nesneyi JSON formatına dönüştürüp,
                                                                                   //belirtilen anahtar (key) ile session'a kaydeder.
                                                                                   //key: JSON verisini kaydedeceğiniz anahtar.
                                                                                   //value: Session'a kaydedilecek olan nesne..
        {
            session.SetString(key,JsonConvert.SerializeObject(value));
        }

        public static T? GetJson<T>(this ISession session,string key) where T : class
        {
            var data=session.GetString(key);

            return data == null ? default(T) : JsonConvert.DeserializeObject<T>(data);
        }

    }
}

//SetJson metodu ile nesneleri JSON formatında session'a kaydedebilirsiniz.
//GetJson metodu ile JSON formatındaki veriyi session'dan alıp, nesneleri doğru türde geri döndürebilirsiniz. Kullanıcı sepetini görmek istediğinde, session'dan veriyi alabiliriz:


//JSON kullanarak, karmaşık nesneleri (örneğin bir ürün listesi) kolayca saklayabiliriz.