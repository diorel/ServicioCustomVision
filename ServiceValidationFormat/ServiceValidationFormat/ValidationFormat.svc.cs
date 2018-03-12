using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Drawing;
using System.IO;
using System.Drawing.Imaging;
using System.Web;

using Spire.Pdf;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using Newtonsoft.Json;





namespace ServiceValidationFormat
{
    // NOTA: puede usar el comando "Rename" del menú "Refactorizar" para cambiar el nombre de clase "Service1" en el código, en svc y en el archivo de configuración.
    // NOTE: para iniciar el Cliente de prueba WCF para probar este servicio, seleccione Service1.svc o Service1.svc.cs en el Explorador de soluciones e inicie la depuración.
    public class Service1 : IValidationFormat
    {

        public class Consulta
        {
            public string Id { get; set; }
            public string Project { get; set; }
            public string Iteration { get; set; }
            public string Created { get; set; }
            public List<Predictions> Predictions { get; set; }
        }


        public class Predictions
        {
            public string TagId { get; set; }
            public string Tag { get; set; }
            public double Probability { get; set; }
        }

        static byte[] GetImageAsByteArray(string imageFilePath)
        {
            FileStream fileStream = new FileStream(imageFilePath, FileMode.Open, FileAccess.Read);
            BinaryReader binaryReader = new BinaryReader(fileStream);
            return binaryReader.ReadBytes((int)fileStream.Length);
        }


        private static ImageCodecInfo GetEncoderInfo(String mimeType)
        {
            int j;
            ImageCodecInfo[] encoders;
            encoders = ImageCodecInfo.GetImageEncoders();
            for (j = 0; j < encoders.Length; ++j)
            {
                if (encoders[j].MimeType == mimeType)
                    return encoders[j];
            }
            return null;
        }


        public async Task<string> ValidarFormatoINE(byte[] ByteArray, DocTo Formato)
        {





            var client = new HttpClient();
            string aprobada = "true";
            string rechazada = "false";
            string evaluacion = "";
            string KeyIA = System.Web.Configuration.WebConfigurationManager.AppSettings["Key"];
            string SisteURL = System.Web.Configuration.WebConfigurationManager.AppSettings["url"];
            int Escala = Convert.ToInt32(System.Web.Configuration.WebConfigurationManager.AppSettings["Escala"]);
            //string extension;



            //En este bloque se envía la url y Prediction-Ke a la api de  cognitive de Microsoft sin estos datos no se puede hacer peticiones a la API

            //client.DefaultRequestHeaders.Add("Prediction-Key", "559018cc3d434cef8095da2e8b8dd30c");
            //string url = "https://southcentralus.api.cognitive.microsoft.com/customvision/v1.1/Prediction/ee601a04-1f53-4b5a-91bb-e5c276ab7832/image?iterationId=c3dcc62f-485d-4f60-bfe1-cfd73befb5f6";

            client.DefaultRequestHeaders.Add("Prediction-Key", KeyIA);
            string url = SisteURL;

            HttpResponseMessage response;

            int ImageSize = ByteArray.Length;

            // Aquí se realiza una evaluación del tamaño de la imagen si excede más de 4MB la comprimirá a un 50 %

            if (ImageSize >= 4194304)
            {

                MemoryStream msz = new MemoryStream();
                Bitmap bmp;
                var ms = new MemoryStream(ByteArray);
                bmp = new Bitmap(ms);

                ImageCodecInfo myImageCodecInfo = GetEncoderInfo("image/jpeg");

                System.Drawing.Imaging.Encoder myEncoder =
                    System.Drawing.Imaging.Encoder.Quality;


                EncoderParameters myEncoderParameters = new EncoderParameters(1);

                //En esta parte indicamos el grado de compresion de la imagen ene ste caso tendra un 50% que se indica con  (50L)
                EncoderParameter myEncoderParameter = new EncoderParameter(myEncoder, 50L);
                myEncoderParameters.Param[0] = myEncoderParameter;
                bmp.Save(msz, myImageCodecInfo, myEncoderParameters);
                byte[] ByteArrayComprimido = msz.GetBuffer();

                int sizecomprimido = ByteArrayComprimido.Length;

                using (var content = new ByteArrayContent(ByteArrayComprimido))
                {
                    content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

                    try
                    {
                        //En esta parte se envía la petición a la IA DE Microsoft para realizar la evaluación
                        response = await client.PostAsync(url, content);

                        Consulta model = null;

                        var respuesta = response.Content.ReadAsStringAsync();

                        respuesta.Wait();

                        model = (Consulta)JsonConvert.DeserializeObject(respuesta.Result.ToString(), new Consulta().GetType());


                        var Descripcion1 = (from cust in model.Predictions
                                            where cust.Tag == "Documentos"
                                            select new
                                            {
                                                Probabilidad = cust.Probability.ToString("P1")
                                            }).ToList().FirstOrDefault().Probabilidad.ToString();


                        var Descripcion2 = (from cust in model.Predictions
                                            where cust.Tag == "INE"
                                            select new
                                            {
                                                Probabilidad = cust.Probability.ToString("P1")
                                            }).ToList().FirstOrDefault().Probabilidad.ToString();

                        string CadenaNumero = Convert.ToString(Descripcion2);
                        string NumeroCadena = CadenaNumero.Replace("%", "");
                        double INE = Convert.ToDouble(NumeroCadena);

                        //Se realiza la evaluación si el porcentaje de porbabilidad no excede del 60% la imagen no será valida 

                        if (INE >= Escala)
                        {
                            evaluacion = aprobada;
                        }
                        else
                        {
                            evaluacion = rechazada;
                        }
                    }
                    catch (Exception ex)
                    {

                        System.Diagnostics.Debug.WriteLine(ex.InnerException.Message);
                    }

                }

            }
            else
            {
                using (var content = new ByteArrayContent(ByteArray))
                {
                    content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

                    try
                    {
                        response = await client.PostAsync(url, content);

                        Consulta model = null;

                        var respuesta = response.Content.ReadAsStringAsync();

                        respuesta.Wait();

                        model = (Consulta)JsonConvert.DeserializeObject(respuesta.Result.ToString(), new Consulta().GetType());



                        var Descripcion1 = (from cust in model.Predictions
                                            where cust.Tag == "Documentos"
                                            select new
                                            {
                                                Probabilidad = cust.Probability.ToString("P1")
                                            }).ToList().FirstOrDefault().Probabilidad.ToString();



                        var Descripcion2 = (from cust in model.Predictions
                                            where cust.Tag == "INE"
                                            select new
                                            {
                                                Probabilidad = cust.Probability.ToString("P1")
                                            }).ToList().FirstOrDefault().Probabilidad.ToString();

                        string CadenaNumero = Convert.ToString(Descripcion2);
                        string NumeroCadena = CadenaNumero.Replace("%", "");
                        double INE = Convert.ToDouble(NumeroCadena);

                        if (INE >= Escala)
                        {
                            evaluacion = aprobada;
                        }
                        else
                        {
                            evaluacion = rechazada;
                        }
                    }
                    catch (Exception ex)
                    {

                        System.Diagnostics.Debug.WriteLine(ex.InnerException.Message);
                    }
                }
            }
            return (evaluacion);

        }

        }
}
