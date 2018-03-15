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


        public async Task<string> ValidarFormato(byte[] ByteArray, DocTo Formato, string Extencion)
        {

            PdfDocument doc = new PdfDocument();
            MemoryStream msz4 = new MemoryStream();

            var client = new HttpClient();
            string aprobada = "true";
            string rechazada = "false";
            string evaluacion = "";

            //INE por los dos lados 
            string KeyIA = System.Web.Configuration.WebConfigurationManager.AppSettings["Key"];
            string SisteURL = System.Web.Configuration.WebConfigurationManager.AppSettings["url"];
            //Anverso IFE-01 
            string KeyIAanversoIFE = System.Web.Configuration.WebConfigurationManager.AppSettings["KeyIAanversoIFE"];
            string SisteURLanversoIFE = System.Web.Configuration.WebConfigurationManager.AppSettings["SisteURLanversoIFE"];
            //Anverso INE-02
            string KeyIAanversoINE = System.Web.Configuration.WebConfigurationManager.AppSettings["KeyIAanversoINE"];
            string SisteURLanversoINE = System.Web.Configuration.WebConfigurationManager.AppSettings["SisteURLanversoINE"];
            //Reverso IFE-03
            string KeyIAreversoIFE = System.Web.Configuration.WebConfigurationManager.AppSettings["KeyIAreversoIFE"];
            string SisteURLreversoIFE = System.Web.Configuration.WebConfigurationManager.AppSettings["SisteURLreversoIFE"];
            //Reverso INE-04
            string KeyIAreversoINE = System.Web.Configuration.WebConfigurationManager.AppSettings["KeyIAreversoINE"];
            string SisteURLreversoINE = System.Web.Configuration.WebConfigurationManager.AppSettings["SisteURLreversoINE"];
            //Pasaporte-05
            string KeyIAPasaporte = System.Web.Configuration.WebConfigurationManager.AppSettings["KeyIAPasaporte"];
            string SisteURLPasaporte = System.Web.Configuration.WebConfigurationManager.AppSettings["SisteURLPasaporte"];
            //CFE-06
            string KeyIACFE = System.Web.Configuration.WebConfigurationManager.AppSettings["KeyIACFE"];
            string SisteURLCFE = System.Web.Configuration.WebConfigurationManager.AppSettings["SisteURLCFE"];
            //Telmex-07
            string KeyIATelmex = System.Web.Configuration.WebConfigurationManager.AppSettings["KeyIATelmex"];
            string SisteURLTelmex = System.Web.Configuration.WebConfigurationManager.AppSettings["SisteURLTelmex"];
            //FormatoAu-08
            string KeyIAFormatoAu = System.Web.Configuration.WebConfigurationManager.AppSettings["KeyIAFormatoAu"];
            string SisteURLFormatoAu = System.Web.Configuration.WebConfigurationManager.AppSettings["SisteURLFormatoAu"];


            //Escala de aprobacion de formatos 
            int Escala = Convert.ToInt32(System.Web.Configuration.WebConfigurationManager.AppSettings["Escala"]);



            // En esta parte valida el formato si es PFD o JPG 
            //BloqueImagen-01

            if (Extencion.Equals(".pdf"))
            {
                doc.LoadFromBytes(ByteArray);
                Image x = doc.SaveAsImage(0);
                x.Save(msz4, ImageFormat.Jpeg);
                byte[] ByteArrayIMG = msz4.GetBuffer();

                switch (Formato)
                {
                    case DocTo.anversoINE:
                    case DocTo.anversoIFE:

                        //Anverso INE-02

                        if (Formato.Equals(DocTo.anversoINE))
                        {
                            client.DefaultRequestHeaders.Add("Prediction-Key", KeyIAanversoINE);
                            string url02 = SisteURLanversoINE;

                            //En este bloque se envía la url y Prediction-Key a la api de  cognitive de Microsoft sin estos datos no se puede hacer peticiones a la API

                            HttpResponseMessage response02;

                            int ImageSize02 = ByteArrayIMG.Length;

                            // Aquí se realiza una evaluación del tamaño de la imagen si excede más de 4MB la comprimirá a un 50 %

                            if (ImageSize02 >= 4194304)
                            {

                                MemoryStream msz = new MemoryStream();
                                Bitmap bmp;
                                var ms = new MemoryStream(ByteArrayIMG);
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
                                        response02 = await client.PostAsync(url02, content);

                                        Consulta model = null;

                                        var respuesta = response02.Content.ReadAsStringAsync();

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
                                using (var content = new ByteArrayContent(ByteArrayIMG))
                                {
                                    content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

                                    try
                                    {
                                        response02 = await client.PostAsync(url02, content);

                                        Consulta model = null;

                                        var respuesta = response02.Content.ReadAsStringAsync();

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
                        }
                        else
                        {
                            //Anverso IFE-01 

                            client.DefaultRequestHeaders.Add("Prediction-Key", KeyIAanversoIFE);
                            string url01 = SisteURLanversoIFE;

                            HttpResponseMessage response01;

                            int ImageSize3 = ByteArrayIMG.Length;

                            // Aquí se realiza una evaluación del tamaño de la imagen si excede más de 4MB la comprimirá a un 50 %

                            if (ImageSize3 >= 4194304)
                            {

                                MemoryStream msz = new MemoryStream();
                                Bitmap bmp;
                                var ms = new MemoryStream(ByteArrayIMG);
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
                                        response01 = await client.PostAsync(url01, content);

                                        Consulta model = null;

                                        var respuesta = response01.Content.ReadAsStringAsync();

                                        respuesta.Wait();

                                        model = (Consulta)JsonConvert.DeserializeObject(respuesta.Result.ToString(), new Consulta().GetType());


                                        var Descripcion1 = (from cust in model.Predictions
                                                            where cust.Tag == "Documentos"
                                                            select new
                                                            {
                                                                Probabilidad = cust.Probability.ToString("P1")
                                                            }).ToList().FirstOrDefault().Probabilidad.ToString();


                                        var Descripcion2 = (from cust in model.Predictions
                                                            where cust.Tag == "IFE"
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
                                using (var content = new ByteArrayContent(ByteArrayIMG))
                                {
                                    content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

                                    try
                                    {
                                        response01 = await client.PostAsync(url01, content);

                                        Consulta model = null;

                                        var respuesta = response01.Content.ReadAsStringAsync();

                                        respuesta.Wait();

                                        model = (Consulta)JsonConvert.DeserializeObject(respuesta.Result.ToString(), new Consulta().GetType());



                                        var Descripcion1 = (from cust in model.Predictions
                                                            where cust.Tag == "Documentos"
                                                            select new
                                                            {
                                                                Probabilidad = cust.Probability.ToString("P1")
                                                            }).ToList().FirstOrDefault().Probabilidad.ToString();



                                        var Descripcion2 = (from cust in model.Predictions
                                                            where cust.Tag == "IFE"
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

                        }
                        break;
                    case DocTo.reversoINE:
                    case DocTo.reversoIFE:

                        //Reverso INE-04          
                        //if (DocTo.reversoINE.Equals(3))
                        if (Formato.Equals(DocTo.reversoINE))
                        {
                            // Se ingresa La Key del proyecto -INE

                            client.DefaultRequestHeaders.Add("Prediction-Key", KeyIAreversoINE);
                            string url04 = SisteURLreversoINE;

                            //En este bloque se envía la url y Prediction-Key a la api de  cognitive de Microsoft sin estos datos no se puede hacer peticiones a la API

                            HttpResponseMessage response04;

                            int ImageSize4 = ByteArrayIMG.Length;

                            // Aquí se realiza una evaluación del tamaño de la imagen si excede más de 4MB la comprimirá a un 50 %

                            if (ImageSize4 >= 4194304)
                            {

                                MemoryStream msz = new MemoryStream();
                                Bitmap bmp;
                                var ms = new MemoryStream(ByteArrayIMG);
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
                                        response04 = await client.PostAsync(url04, content);

                                        Consulta model = null;

                                        var respuesta = response04.Content.ReadAsStringAsync();

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
                                using (var content = new ByteArrayContent(ByteArrayIMG))
                                {
                                    content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

                                    try
                                    {
                                        response04 = await client.PostAsync(url04, content);

                                        Consulta model = null;

                                        var respuesta = response04.Content.ReadAsStringAsync();

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
                        }
                        else
                        {
                            //Reverso IFE-03
                            client.DefaultRequestHeaders.Add("Prediction-Key", KeyIAreversoIFE);
                            string url03 = SisteURLreversoIFE;

                            HttpResponseMessage response03;

                            int ImageSize = ByteArrayIMG.Length;

                            // Aquí se realiza una evaluación del tamaño de la imagen si excede más de 4MB la comprimirá a un 50 %

                            if (ImageSize >= 4194304)
                            {

                                MemoryStream msz = new MemoryStream();
                                Bitmap bmp;
                                var ms = new MemoryStream(ByteArrayIMG);
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
                                        response03 = await client.PostAsync(url03, content);

                                        Consulta model = null;

                                        var respuesta = response03.Content.ReadAsStringAsync();

                                        respuesta.Wait();

                                        model = (Consulta)JsonConvert.DeserializeObject(respuesta.Result.ToString(), new Consulta().GetType());


                                        var Descripcion1 = (from cust in model.Predictions
                                                            where cust.Tag == "Documentos"
                                                            select new
                                                            {
                                                                Probabilidad = cust.Probability.ToString("P1")
                                                            }).ToList().FirstOrDefault().Probabilidad.ToString();


                                        var Descripcion2 = (from cust in model.Predictions
                                                            where cust.Tag == "IFE"
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
                                using (var content = new ByteArrayContent(ByteArrayIMG))
                                {
                                    content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

                                    try
                                    {
                                        response03 = await client.PostAsync(url03, content);

                                        Consulta model = null;

                                        var respuesta = response03.Content.ReadAsStringAsync();

                                        respuesta.Wait();

                                        model = (Consulta)JsonConvert.DeserializeObject(respuesta.Result.ToString(), new Consulta().GetType());



                                        var Descripcion1 = (from cust in model.Predictions
                                                            where cust.Tag == "Documentos"
                                                            select new
                                                            {
                                                                Probabilidad = cust.Probability.ToString("P1")
                                                            }).ToList().FirstOrDefault().Probabilidad.ToString();



                                        var Descripcion2 = (from cust in model.Predictions
                                                            where cust.Tag == "IFE"
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

                        }
                        break;

                    case DocTo.pasaporte:

                        //Pasaporte-05

                        client.DefaultRequestHeaders.Add("Prediction-Key", KeyIAPasaporte);
                        string url5 = SisteURLPasaporte;

                        HttpResponseMessage response5;

                        int ImageSize5 = ByteArrayIMG.Length;

                        // Aquí se realiza una evaluación del tamaño de la imagen si excede más de 4MB la comprimirá a un 50 %

                        if (ImageSize5 >= 4194304)
                        {

                            MemoryStream msz = new MemoryStream();
                            Bitmap bmp;
                            var ms = new MemoryStream(ByteArrayIMG);
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
                                    response5 = await client.PostAsync(url5, content);

                                    Consulta model = null;

                                    var respuesta = response5.Content.ReadAsStringAsync();

                                    respuesta.Wait();

                                    model = (Consulta)JsonConvert.DeserializeObject(respuesta.Result.ToString(), new Consulta().GetType());


                                    var Descripcion1 = (from cust in model.Predictions
                                                        where cust.Tag == "Documentos"
                                                        select new
                                                        {
                                                            Probabilidad = cust.Probability.ToString("P1")
                                                        }).ToList().FirstOrDefault().Probabilidad.ToString();


                                    var Descripcion2 = (from cust in model.Predictions
                                                        where cust.Tag == "Pasaporte"
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

                            //modificiacion bite 

                            using (var content = new ByteArrayContent(ByteArrayIMG))
                            {
                                content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

                                try
                                {
                                    response5 = await client.PostAsync(url5, content);

                                    Consulta model = null;

                                    var respuesta = response5.Content.ReadAsStringAsync();

                                    respuesta.Wait();

                                    model = (Consulta)JsonConvert.DeserializeObject(respuesta.Result.ToString(), new Consulta().GetType());



                                    var Descripcion1 = (from cust in model.Predictions
                                                        where cust.Tag == "Documentos"
                                                        select new
                                                        {
                                                            Probabilidad = cust.Probability.ToString("P1")
                                                        }).ToList().FirstOrDefault().Probabilidad.ToString();



                                    var Descripcion2 = (from cust in model.Predictions
                                                        where cust.Tag == "Pasaporte"
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

                        break;

                    case DocTo.CFE:

                        //CFE-06

                        client.DefaultRequestHeaders.Add("Prediction-Key", KeyIACFE);
                        string url06 = SisteURLCFE;

                        HttpResponseMessage response06;

                        int ImageSize06 = ByteArrayIMG.Length;

                        // Aquí se realiza una evaluación del tamaño de la imagen si excede más de 4MB la comprimirá a un 50 %

                        if (ImageSize06 >= 4194304)
                        {

                            MemoryStream msz = new MemoryStream();
                            Bitmap bmp;
                            var ms = new MemoryStream(ByteArrayIMG);
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
                                    response06 = await client.PostAsync(url06, content);

                                    Consulta model = null;

                                    var respuesta = response06.Content.ReadAsStringAsync();

                                    respuesta.Wait();

                                    model = (Consulta)JsonConvert.DeserializeObject(respuesta.Result.ToString(), new Consulta().GetType());


                                    var Descripcion1 = (from cust in model.Predictions
                                                        where cust.Tag == "Documentos"
                                                        select new
                                                        {
                                                            Probabilidad = cust.Probability.ToString("P1")
                                                        }).ToList().FirstOrDefault().Probabilidad.ToString();


                                    var Descripcion2 = (from cust in model.Predictions
                                                        where cust.Tag == "CFE"
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
                            using (var content = new ByteArrayContent(ByteArrayIMG))
                            {
                                content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

                                try
                                {
                                    response06 = await client.PostAsync(url06, content);

                                    Consulta model = null;

                                    var respuesta = response06.Content.ReadAsStringAsync();

                                    respuesta.Wait();

                                    model = (Consulta)JsonConvert.DeserializeObject(respuesta.Result.ToString(), new Consulta().GetType());



                                    var Descripcion1 = (from cust in model.Predictions
                                                        where cust.Tag == "Documentos"
                                                        select new
                                                        {
                                                            Probabilidad = cust.Probability.ToString("P1")
                                                        }).ToList().FirstOrDefault().Probabilidad.ToString();



                                    var Descripcion2 = (from cust in model.Predictions
                                                        where cust.Tag == "CFE"
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


                        break;

                    case DocTo.Telmex:

                        // Telmex-07


                        client.DefaultRequestHeaders.Add("Prediction-Key", KeyIATelmex);
                        string url07 = SisteURLTelmex;

                        HttpResponseMessage response07;

                        int ImageSize07 = ByteArrayIMG.Length;

                        // Aquí se realiza una evaluación del tamaño de la imagen si excede más de 4MB la comprimirá a un 50 %

                        if (ImageSize07 >= 4194304)
                        {

                            MemoryStream msz = new MemoryStream();
                            Bitmap bmp;
                            var ms = new MemoryStream(ByteArrayIMG);
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
                                    response07 = await client.PostAsync(url07, content);

                                    Consulta model = null;

                                    var respuesta = response07.Content.ReadAsStringAsync();

                                    respuesta.Wait();

                                    model = (Consulta)JsonConvert.DeserializeObject(respuesta.Result.ToString(), new Consulta().GetType());


                                    var Descripcion1 = (from cust in model.Predictions
                                                        where cust.Tag == "Documentos"
                                                        select new
                                                        {
                                                            Probabilidad = cust.Probability.ToString("P1")
                                                        }).ToList().FirstOrDefault().Probabilidad.ToString();


                                    var Descripcion2 = (from cust in model.Predictions
                                                        where cust.Tag == "TELMEX"
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
                            using (var content = new ByteArrayContent(ByteArrayIMG))
                            {
                                content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

                                try
                                {
                                    response07 = await client.PostAsync(url07, content);

                                    Consulta model = null;

                                    var respuesta = response07.Content.ReadAsStringAsync();

                                    respuesta.Wait();

                                    model = (Consulta)JsonConvert.DeserializeObject(respuesta.Result.ToString(), new Consulta().GetType());



                                    var Descripcion1 = (from cust in model.Predictions
                                                        where cust.Tag == "Documentos"
                                                        select new
                                                        {
                                                            Probabilidad = cust.Probability.ToString("P1")
                                                        }).ToList().FirstOrDefault().Probabilidad.ToString();



                                    var Descripcion2 = (from cust in model.Predictions
                                                        where cust.Tag == "TELMEX"
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

                        break;
                    case DocTo.FormatoAu:

                        //FormatoAu-08

                        client.DefaultRequestHeaders.Add("Prediction-Key", KeyIAFormatoAu);
                        string url08 = SisteURLFormatoAu;

                        HttpResponseMessage response08;

                        int ImageSize08 = ByteArrayIMG.Length;

                        // Aquí se realiza una evaluación del tamaño de la imagen si excede más de 4MB la comprimirá a un 50 %

                        if (ImageSize08 >= 4194304)
                        {

                            MemoryStream msz = new MemoryStream();
                            Bitmap bmp;
                            var ms = new MemoryStream(ByteArrayIMG);
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
                                    response08 = await client.PostAsync(url08, content);

                                    Consulta model = null;

                                    var respuesta = response08.Content.ReadAsStringAsync();

                                    respuesta.Wait();

                                    model = (Consulta)JsonConvert.DeserializeObject(respuesta.Result.ToString(), new Consulta().GetType());


                                    var Descripcion1 = (from cust in model.Predictions
                                                        where cust.Tag == "Documnetos"
                                                        select new
                                                        {
                                                            Probabilidad = cust.Probability.ToString("P1")
                                                        }).ToList().FirstOrDefault().Probabilidad.ToString();


                                    var Descripcion2 = (from cust in model.Predictions
                                                        where cust.Tag == "AV"
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
                            using (var content = new ByteArrayContent(ByteArrayIMG))
                            {
                                content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

                                try
                                {
                                    response08 = await client.PostAsync(url08, content);

                                    Consulta model = null;

                                    var respuesta = response08.Content.ReadAsStringAsync();

                                    respuesta.Wait();

                                    model = (Consulta)JsonConvert.DeserializeObject(respuesta.Result.ToString(), new Consulta().GetType());



                                    var Descripcion1 = (from cust in model.Predictions
                                                        where cust.Tag == "Documnetos"
                                                        select new
                                                        {
                                                            Probabilidad = cust.Probability.ToString("P1")
                                                        }).ToList().FirstOrDefault().Probabilidad.ToString();



                                    var Descripcion2 = (from cust in model.Predictions
                                                        where cust.Tag == "AV"
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

                        break;
                }

            }

            ///********************** esta parte se ejecuta cuando solo es imagen  BloqueImagen-01
            else
            {

                switch (Formato)
                {
                    case DocTo.anversoINE:
                    case DocTo.anversoIFE:

                        //Anverso INE-02

                        if (Formato.Equals(DocTo.anversoINE))
                        {
                            client.DefaultRequestHeaders.Add("Prediction-Key", KeyIAanversoINE);
                            string url02 = SisteURLanversoINE;

                            //En este bloque se envía la url y Prediction-Key a la api de  cognitive de Microsoft sin estos datos no se puede hacer peticiones a la API

                            HttpResponseMessage response02;

                            int ImageSize02 = ByteArray.Length;

                            // Aquí se realiza una evaluación del tamaño de la imagen si excede más de 4MB la comprimirá a un 50 %

                            if (ImageSize02 >= 4194304)
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
                                        response02 = await client.PostAsync(url02, content);

                                        Consulta model = null;

                                        var respuesta = response02.Content.ReadAsStringAsync();

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
                                        response02 = await client.PostAsync(url02, content);

                                        Consulta model = null;

                                        var respuesta = response02.Content.ReadAsStringAsync();

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
                        }
                        else
                        {
                            //Anverso IFE-01 

                            client.DefaultRequestHeaders.Add("Prediction-Key", KeyIAanversoIFE);
                            string url01 = SisteURLanversoIFE;

                            HttpResponseMessage response01;

                            int ImageSize3 = ByteArray.Length;

                            // Aquí se realiza una evaluación del tamaño de la imagen si excede más de 4MB la comprimirá a un 50 %

                            if (ImageSize3 >= 4194304)
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
                                        response01 = await client.PostAsync(url01, content);

                                        Consulta model = null;

                                        var respuesta = response01.Content.ReadAsStringAsync();

                                        respuesta.Wait();

                                        model = (Consulta)JsonConvert.DeserializeObject(respuesta.Result.ToString(), new Consulta().GetType());


                                        var Descripcion1 = (from cust in model.Predictions
                                                            where cust.Tag == "Documentos"
                                                            select new
                                                            {
                                                                Probabilidad = cust.Probability.ToString("P1")
                                                            }).ToList().FirstOrDefault().Probabilidad.ToString();


                                        var Descripcion2 = (from cust in model.Predictions
                                                            where cust.Tag == "IFE"
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
                                        response01 = await client.PostAsync(url01, content);

                                        Consulta model = null;

                                        var respuesta = response01.Content.ReadAsStringAsync();

                                        respuesta.Wait();

                                        model = (Consulta)JsonConvert.DeserializeObject(respuesta.Result.ToString(), new Consulta().GetType());



                                        var Descripcion1 = (from cust in model.Predictions
                                                            where cust.Tag == "Documentos"
                                                            select new
                                                            {
                                                                Probabilidad = cust.Probability.ToString("P1")
                                                            }).ToList().FirstOrDefault().Probabilidad.ToString();



                                        var Descripcion2 = (from cust in model.Predictions
                                                            where cust.Tag == "IFE"
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

                        }
                        break;
                    case DocTo.reversoINE:
                    case DocTo.reversoIFE:

                        //Reverso INE-04          
                        //if (DocTo.reversoINE.Equals(3))
                        if (Formato.Equals(DocTo.reversoINE))
                        {
                            // Se ingresa La Key del proyecto -INE

                            client.DefaultRequestHeaders.Add("Prediction-Key", KeyIAreversoINE);
                            string url04 = SisteURLreversoINE;

                            //En este bloque se envía la url y Prediction-Key a la api de  cognitive de Microsoft sin estos datos no se puede hacer peticiones a la API

                            HttpResponseMessage response04;

                            int ImageSize4 = ByteArray.Length;

                            // Aquí se realiza una evaluación del tamaño de la imagen si excede más de 4MB la comprimirá a un 50 %

                            if (ImageSize4 >= 4194304)
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
                                        response04 = await client.PostAsync(url04, content);

                                        Consulta model = null;

                                        var respuesta = response04.Content.ReadAsStringAsync();

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
                                        response04 = await client.PostAsync(url04, content);

                                        Consulta model = null;

                                        var respuesta = response04.Content.ReadAsStringAsync();

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
                        }
                        else
                        {
                            //Reverso IFE-03
                            client.DefaultRequestHeaders.Add("Prediction-Key", KeyIAreversoIFE);
                            string url03 = SisteURLreversoIFE;

                            HttpResponseMessage response03;

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
                                        response03 = await client.PostAsync(url03, content);

                                        Consulta model = null;

                                        var respuesta = response03.Content.ReadAsStringAsync();

                                        respuesta.Wait();

                                        model = (Consulta)JsonConvert.DeserializeObject(respuesta.Result.ToString(), new Consulta().GetType());


                                        var Descripcion1 = (from cust in model.Predictions
                                                            where cust.Tag == "Documentos"
                                                            select new
                                                            {
                                                                Probabilidad = cust.Probability.ToString("P1")
                                                            }).ToList().FirstOrDefault().Probabilidad.ToString();


                                        var Descripcion2 = (from cust in model.Predictions
                                                            where cust.Tag == "IFE"
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
                                        response03 = await client.PostAsync(url03, content);

                                        Consulta model = null;

                                        var respuesta = response03.Content.ReadAsStringAsync();

                                        respuesta.Wait();

                                        model = (Consulta)JsonConvert.DeserializeObject(respuesta.Result.ToString(), new Consulta().GetType());



                                        var Descripcion1 = (from cust in model.Predictions
                                                            where cust.Tag == "Documentos"
                                                            select new
                                                            {
                                                                Probabilidad = cust.Probability.ToString("P1")
                                                            }).ToList().FirstOrDefault().Probabilidad.ToString();



                                        var Descripcion2 = (from cust in model.Predictions
                                                            where cust.Tag == "IFE"
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

                        }
                        break;

                    case DocTo.pasaporte:

                        //Pasaporte-05

                        client.DefaultRequestHeaders.Add("Prediction-Key", KeyIAPasaporte);
                        string url5 = SisteURLPasaporte;

                        HttpResponseMessage response5;

                        int ImageSize5 = ByteArray.Length;

                        // Aquí se realiza una evaluación del tamaño de la imagen si excede más de 4MB la comprimirá a un 50 %

                        if (ImageSize5 >= 4194304)
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
                                    response5 = await client.PostAsync(url5, content);

                                    Consulta model = null;

                                    var respuesta = response5.Content.ReadAsStringAsync();

                                    respuesta.Wait();

                                    model = (Consulta)JsonConvert.DeserializeObject(respuesta.Result.ToString(), new Consulta().GetType());


                                    var Descripcion1 = (from cust in model.Predictions
                                                        where cust.Tag == "Documentos"
                                                        select new
                                                        {
                                                            Probabilidad = cust.Probability.ToString("P1")
                                                        }).ToList().FirstOrDefault().Probabilidad.ToString();


                                    var Descripcion2 = (from cust in model.Predictions
                                                        where cust.Tag == "Pasaporte"
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

                            //modificiacion bite 

                            using (var content = new ByteArrayContent(ByteArray))
                            {
                                content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

                                try
                                {
                                    response5 = await client.PostAsync(url5, content);

                                    Consulta model = null;

                                    var respuesta = response5.Content.ReadAsStringAsync();

                                    respuesta.Wait();

                                    model = (Consulta)JsonConvert.DeserializeObject(respuesta.Result.ToString(), new Consulta().GetType());



                                    var Descripcion1 = (from cust in model.Predictions
                                                        where cust.Tag == "Documentos"
                                                        select new
                                                        {
                                                            Probabilidad = cust.Probability.ToString("P1")
                                                        }).ToList().FirstOrDefault().Probabilidad.ToString();



                                    var Descripcion2 = (from cust in model.Predictions
                                                        where cust.Tag == "Pasaporte"
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

                        break;

                    case DocTo.CFE:

                        //CFE-06

                        client.DefaultRequestHeaders.Add("Prediction-Key", KeyIACFE);
                        string url06 = SisteURLCFE;

                        HttpResponseMessage response06;

                        int ImageSize06 = ByteArray.Length;

                        // Aquí se realiza una evaluación del tamaño de la imagen si excede más de 4MB la comprimirá a un 50 %

                        if (ImageSize06 >= 4194304)
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
                                    response06 = await client.PostAsync(url06, content);

                                    Consulta model = null;

                                    var respuesta = response06.Content.ReadAsStringAsync();

                                    respuesta.Wait();

                                    model = (Consulta)JsonConvert.DeserializeObject(respuesta.Result.ToString(), new Consulta().GetType());


                                    var Descripcion1 = (from cust in model.Predictions
                                                        where cust.Tag == "Documentos"
                                                        select new
                                                        {
                                                            Probabilidad = cust.Probability.ToString("P1")
                                                        }).ToList().FirstOrDefault().Probabilidad.ToString();


                                    var Descripcion2 = (from cust in model.Predictions
                                                        where cust.Tag == "CFE"
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
                                    response06 = await client.PostAsync(url06, content);

                                    Consulta model = null;

                                    var respuesta = response06.Content.ReadAsStringAsync();

                                    respuesta.Wait();

                                    model = (Consulta)JsonConvert.DeserializeObject(respuesta.Result.ToString(), new Consulta().GetType());



                                    var Descripcion1 = (from cust in model.Predictions
                                                        where cust.Tag == "Documentos"
                                                        select new
                                                        {
                                                            Probabilidad = cust.Probability.ToString("P1")
                                                        }).ToList().FirstOrDefault().Probabilidad.ToString();



                                    var Descripcion2 = (from cust in model.Predictions
                                                        where cust.Tag == "CFE"
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


                        break;

                    case DocTo.Telmex:

                        // Telmex-07


                        client.DefaultRequestHeaders.Add("Prediction-Key", KeyIATelmex);
                        string url07 = SisteURLTelmex;

                        HttpResponseMessage response07;

                        int ImageSize07 = ByteArray.Length;

                        // Aquí se realiza una evaluación del tamaño de la imagen si excede más de 4MB la comprimirá a un 50 %

                        if (ImageSize07 >= 4194304)
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
                                    response07 = await client.PostAsync(url07, content);

                                    Consulta model = null;

                                    var respuesta = response07.Content.ReadAsStringAsync();

                                    respuesta.Wait();

                                    model = (Consulta)JsonConvert.DeserializeObject(respuesta.Result.ToString(), new Consulta().GetType());


                                    var Descripcion1 = (from cust in model.Predictions
                                                        where cust.Tag == "Documentos"
                                                        select new
                                                        {
                                                            Probabilidad = cust.Probability.ToString("P1")
                                                        }).ToList().FirstOrDefault().Probabilidad.ToString();


                                    var Descripcion2 = (from cust in model.Predictions
                                                        where cust.Tag == "TELMEX"
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
                                    response07 = await client.PostAsync(url07, content);

                                    Consulta model = null;

                                    var respuesta = response07.Content.ReadAsStringAsync();

                                    respuesta.Wait();

                                    model = (Consulta)JsonConvert.DeserializeObject(respuesta.Result.ToString(), new Consulta().GetType());



                                    var Descripcion1 = (from cust in model.Predictions
                                                        where cust.Tag == "Documentos"
                                                        select new
                                                        {
                                                            Probabilidad = cust.Probability.ToString("P1")
                                                        }).ToList().FirstOrDefault().Probabilidad.ToString();



                                    var Descripcion2 = (from cust in model.Predictions
                                                        where cust.Tag == "TELMEX"
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

                        break;
                    case DocTo.FormatoAu:

                        //FormatoAu-08

                        client.DefaultRequestHeaders.Add("Prediction-Key", KeyIAFormatoAu);
                        string url08 = SisteURLFormatoAu;

                        HttpResponseMessage response08;

                        int ImageSize08 = ByteArray.Length;

                        // Aquí se realiza una evaluación del tamaño de la imagen si excede más de 4MB la comprimirá a un 50 %

                        if (ImageSize08 >= 4194304)
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
                                    response08 = await client.PostAsync(url08, content);

                                    Consulta model = null;

                                    var respuesta = response08.Content.ReadAsStringAsync();

                                    respuesta.Wait();

                                    model = (Consulta)JsonConvert.DeserializeObject(respuesta.Result.ToString(), new Consulta().GetType());


                                    var Descripcion1 = (from cust in model.Predictions
                                                        where cust.Tag == "Documnetos"
                                                        select new
                                                        {
                                                            Probabilidad = cust.Probability.ToString("P1")
                                                        }).ToList().FirstOrDefault().Probabilidad.ToString();


                                    var Descripcion2 = (from cust in model.Predictions
                                                        where cust.Tag == "AV"
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
                                    response08 = await client.PostAsync(url08, content);

                                    Consulta model = null;

                                    var respuesta = response08.Content.ReadAsStringAsync();

                                    respuesta.Wait();

                                    model = (Consulta)JsonConvert.DeserializeObject(respuesta.Result.ToString(), new Consulta().GetType());



                                    var Descripcion1 = (from cust in model.Predictions
                                                        where cust.Tag == "Documnetos"
                                                        select new
                                                        {
                                                            Probabilidad = cust.Probability.ToString("P1")
                                                        }).ToList().FirstOrDefault().Probabilidad.ToString();



                                    var Descripcion2 = (from cust in model.Predictions
                                                        where cust.Tag == "AV"
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

                        break;
                }

            }
        
            return (evaluacion);

        }






        public string ConvertirPDFaImagen(string Ruta)
        {
            PdfDocument doc = new PdfDocument();
            string extension;

            extension = Path.GetExtension(Ruta);
            if (extension.Equals(".pdf"))
            {

                doc.LoadFromFile(Ruta);

                Image bmp = doc.SaveAsImage(0);

                Image emf = doc.SaveAsImage(0, Spire.Pdf.Graphics.PdfImageType.Metafile);

                Image zoomImg = new Bitmap((int)(emf.Size.Width * 2), (int)(emf.Size.Height * 2));

                using (Graphics g = Graphics.FromImage(zoomImg))

                {
                    g.ScaleTransform(2.0f, 2.0f);

                    g.DrawImage(emf, new Rectangle(new Point(0, 0), emf.Size), new Rectangle(new Point(0, 0), emf.Size), GraphicsUnit.Pixel);

                    try
                    {
                        // bmp.Save(@"~\\img\\INESantanderIMG.jpg", ImageFormat.Jpeg);
                        //  bmp.Save(@"C:\ConvertPDF\INESantanderIMG.jpg", ImageFormat.Jpeg);
                        //  bmp.Save(@"..\..\..\img\INESantanderIMG.jpg", ImageFormat.Jpeg);
                        bmp.Save(@" C:\Users\rcortes\Documents\GitHub\RedRaul\WcfService1\WcfService1\img", ImageFormat.Jpeg);

                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(ex.InnerException.Message);

                    }
                }

            }
            return null;
        }





    }
}
