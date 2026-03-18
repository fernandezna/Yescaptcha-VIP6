using System;
using System.IO;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;

namespace GoogleOTP
{
    class Program
    {
        private const string clientKey = "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx"; //YesCaptcha的Token值 网站可查
        private const string websiteURL = "https://www.google.com/recaptcha/api2/demo"; //需要破解验证码网站的链接
        private const string websiteKey = "6LfLuZ8aAAAAAK8VxR1Nk1rwA-LEALM99o2r3Ujd"; //需要破解验证码网站的site_key 须替换成自己的 YesCaptcha官网有查询方法
        private const string createTask_URL = "https://api.yescaptcha.com/createTask"; //创建识别任务的API路径
        private const string getTaskResult_URL = "https://api.yescaptcha.com/getTaskResult"; //获取识别结果的API路径
        private const string task_type = "NoCaptchaTaskProxyless";  //根据验证码类型，YesCaptcha官网有查询方法
        static void Main(string[] args)
        {
            string taskid = create_task();  //创建识别任务 获取taskId
            string response = get_response(taskid); //获取识别结果

            //将response填入textarea后提交......
            
        }


        /// <summary>
        /// 创建识别任务 获取taskId
        /// </summary>
        /// <param name="url">createTask_URL</param>
        /// <returns>taskId字符串</returns>
        private static string create_task()
        {
            try
            {
                string data = "{\"clientKey\":\"" + clientKey + "\",\"task\":{\"websiteURL\":\"" + websiteURL + "\",\"websiteKey\":\"" + websiteKey + "\",\"type\":\"" + task_type + "\"}}";
                string result = HttpPost(createTask_URL, data);
                SerializeHelper ser = new SerializeHelper();
                YesCreateTsak yes = ser.JsonDeserialize<YesCreateTsak>(result);
                return yes.taskId;
            }
            catch (Exception)
            {
                throw;
            }
        }


        /// <summary>
        /// 获取识别结果 RecaptchaResponse
        /// 每个任务限制最多120 次请求
        /// 每个任务创建后 5 分钟内可以查询
        /// </summary>
        /// <param name="url">getTaskResult_URL</param>
        /// <param name="taskid">识别任务获取到的taskId</param>
        /// <returns>yes.solution.gRecaptchaResponse</returns>
        private static string get_response(string taskid)
        {
            string data = "{\"clientKey\":\"" + clientKey + "\",\"taskId\":\"" + taskid + "\"}";
            string result = "";
            string gRecaptchaResponse = "";
            for (int i = 0; i < 120; i++)
            {
                result = HttpPost(getTaskResult_URL, data);
                SerializeHelper ser = new SerializeHelper();
                YesGetTaskResult yes = ser.JsonDeserialize<YesGetTaskResult>(result);
                if (yes.errorId == 0) //正在识别或者识别完成
                {
                    if (yes.status == "processing")//正在识别中
                    {
                        //需要做的动作
                    }
                    else
                    {
                        //识别完成
                        gRecaptchaResponse = yes.solution.gRecaptchaResponse;
                        break;
                    }
                }
                else //识别失败
                {
                    //需要做的动作
                }
                i++;
                Thread.Sleep(TimeSpan.FromSeconds(3));//暂停线程三秒
            }
            return gRecaptchaResponse;
        }

        /// <summary>
        /// Http Post提交
        /// </summary>
        /// <param name="posturl"></param>
        /// <param name="postData"></param>
        /// <returns></returns>
        public static string HttpPost(string posturl, string postData)
        {
            Stream outstream = null;
            Stream instream = null;
            StreamReader sr = null;
            HttpWebResponse response = null;
            HttpWebRequest request = null;
            Encoding encoding = System.Text.Encoding.GetEncoding("utf-8");
            byte[] data = encoding.GetBytes(postData);
            // 准备请求...
            try
            {
                // 设置参数
                request = WebRequest.Create(posturl) as HttpWebRequest;
                CookieContainer cookieContainer = new CookieContainer();
                request.CookieContainer = cookieContainer;
                request.AllowAutoRedirect = true;
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = data.Length;
                outstream = request.GetRequestStream();
                outstream.Write(data, 0, data.Length);
                outstream.Close();
                //发送请求并获取相应回应数据
                response = request.GetResponse() as HttpWebResponse;
                //直到request.GetResponse()程序才开始向目标网页发送Post请求
                instream = response.GetResponseStream();
                sr = new StreamReader(instream, encoding);
                //返回结果网页（html）代码
                string content = sr.ReadToEnd();
                string err = string.Empty;
                return content;
            }
            catch (Exception ex)
            {
                string err = ex.Message;
                return string.Empty;
            }
        }
    }


    /// <summary>
    /// CreateTask实体类
    /// </summary>
    public class YesCreateTsak
    {
        public string errorId { get; set; }
        public string errorCode { get; set; }
        public string errorDescription { get; set; }
        public string taskId { get; set; }
    }


    /// <summary>
    /// GetTaskResult实体类
    /// </summary>
    public class YesGetTaskResult
    {
        public int errorId { get; set; }
        public string errorCode { get; set; }
        public string errorDescription { get; set; }
        public string status { get; set; }
        public solution solution { get; set; }
    }


    /// <summary>
    /// solution实体类
    /// </summary>
    public class solution
    {
        public string gRecaptchaResponse { get; set; }
    }


    /// <summary>
    /// 序列化
    /// </summary>
    public class SerializeHelper
    {
        //System.Web.Script.Serialization.JavaScriptSerializer serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
        public readonly static SerializeHelper Instance = new SerializeHelper();

        /// <summary>
        /// 将C#数据实体转化为JSON数据
        /// </summary>
        /// <param name="obj">要转化的数据实体</param>
        /// <returns>JSON格式字符串</returns>
        public string JsonSerialize<T>(T obj)
        {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T));
            MemoryStream stream = new MemoryStream();
            serializer.WriteObject(stream, obj);
            stream.Position = 0;

            StreamReader sr = new StreamReader(stream);
            string resultStr = sr.ReadToEnd();
            sr.Close();
            stream.Close();

            return resultStr;
        }

        /// <summary>
        /// 将JSON数据转化为C#数据实体
        /// </summary>
        /// <param name="json">符合JSON格式的字符串</param>
        /// <returns>T类型的对象</returns>
        public T JsonDeserialize<T>(string json)
        {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T));
            MemoryStream ms = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(json.ToCharArray()));
            T obj = (T)serializer.ReadObject(ms);
            ms.Close();
            return obj;
        }
    }
}
