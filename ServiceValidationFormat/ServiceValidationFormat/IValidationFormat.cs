using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Threading.Tasks;


namespace ServiceValidationFormat
{
    // NOTA: puede usar el comando "Rename" del menú "Refactorizar" para cambiar el nombre de interfaz "IService1" en el código y en el archivo de configuración a la vez.
    [ServiceContract]

    public interface IValidationFormat
    {

        [OperationContract]
        Task<string> ValidarFormato(byte[] ByteArray, DocTo Formato);



        [OperationContract]
        string ConvertirPDFaImagen(string Ruta);



    }



}
