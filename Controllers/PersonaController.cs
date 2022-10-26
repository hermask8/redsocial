using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using Newtonsoft.Json.Linq;
using System.Data;
using redsocialBack.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace redsocialBack.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PersonaController : ControllerBase
    {
        //Importamos la configuración de lo que está en appsetting.json
        private IConfiguration Configuration;
        private readonly string _connectionString;

        //Inicializamos las configuraciones del appsetting.json
        public PersonaController(IConfiguration _configuration)
        {
            Configuration = _configuration;
            //La cadena de conexión de MySQL, este es la instancia de base de datos de azure
            _connectionString = _configuration.GetConnectionString("MySqlConnection");
        }



        //Método Get en donde obtenemos todos los registros de la tabla persona
        //Tipo del método
        [HttpGet]
        //Le indicamos el tipo de respuesta que enviará
        [Produces("application/json")]
        //Le indicamos el path que indicará en las peticiones
        [Route("getPersonas")]
        //[Authorize]
        public IActionResult getPersonas(int id)
        {
            //Abrimos la conexión con la instancia de BD
            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                //Le decimos que será un comando que está alojado en el sp_persona, en este caso es un store procedure
                using MySqlCommand cmd = new MySqlCommand("sp_persona", conn);
                {
                    dynamic data;
                    //Le indicamos el tipo de comando que ejecutara
                    cmd.CommandType = CommandType.StoredProcedure;
                    //Abrimos la conexión
                    conn.Open();
                    //Le indicamos los parámetros del SP, en este caso tienen que se TODOS los que se indicaron en el sp y si no se usan colocarlos igualmente e indicarlo que es un null
                    cmd.Parameters.AddWithValue("@Opcion", 3);
                    cmd.Parameters.AddWithValue("@id_persona", null);                 
                    cmd.Parameters.AddWithValue("@nombre", null);
                    cmd.Parameters.AddWithValue("@apellido", null);
                    cmd.Parameters.AddWithValue("@edad", null);

                    DataSet setter = new DataSet();
                    MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);


                    try
                    {
                        //Verificamos si la consulta es null
                        adapter.Fill(setter, "data");

                        if (setter.Tables["data"] == null)
                        {
                            data = new JObject();
                            data.message = "test";
                            data.response = 1;
                            return Ok(data);
                        }
                    }
                    catch (Exception ex)
                    {

                        return Ok(ex.ToString());
                    }
                    //Verificamos que si la consulta está vacia
                    if (setter.Tables["data"].Rows.Count <= 0)
                    {
                        //Enviamos de respuesta de vacio a la petición
                        JArray vacio = new JArray();
                        return Ok(new { response = vacio });
                    }
                    //Si todo es correcto enviamos la petición, en este caso todos los registros
                    var se = setter.Tables["data"];
                    return Ok(setter.Tables["data"]);
                }
            }

        }


        //Método para insertar un registro
        [HttpPost]
        // [Authorize]
        [Produces("application/json")]
        [Route("insertPersona")]
        //Le indicamos que deserealice lo del body al modelo persona, tiene que venir con el mismo nombre que la clase persona
        public IActionResult InsertarCliente([FromBody] Persona persona)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(_connectionString))
                {
                    using (MySqlCommand cmd = new MySqlCommand("sp_persona", conn))
                    {

                        dynamic data;
                        cmd.CommandType = CommandType.StoredProcedure;

                        //conn.Open();
                        //cmd.Parameters.AddWithValue("@id", 0);

                       cmd.Parameters.AddWithValue("@id_persona", null);

                        cmd.Parameters.AddWithValue("@nombre", persona.nombre);
                        cmd.Parameters.AddWithValue("@apellido", persona.apellido);
                        cmd.Parameters.AddWithValue("@edad", persona.edad);
                        cmd.Parameters.AddWithValue("@Opcion", 1);

                        conn.Open();
                        DataSet setter = new DataSet();
                        MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                        adapter.Fill(setter, "OK");
                        //Siempre mandar un resultado de ok desde la base de datos para saber si hubo respuesta del msimo
                        if (setter.Tables["OK"].Rows.Count > 0 || setter != null)
                        {
                            //Se le manda el mensaje de registro guardado exitosamente
                            data = new JObject();
                            data.response = 4;
                            data.message = "Registro Guardado Exitosamente";
                            return Ok(data);
                        }
                        else
                        {
                            data = new JObject();
                            data.value = 0;
                            data.response = 4;
                            data.message = setter.Tables[0].Rows[0][0];
                            return BadRequest(data);
                        }



                    }
                }
            }
            catch (Exception ex)
            {
                dynamic data = new JObject();
                data.value = ex.ToString();
                data.response = 6;
                data.message = "Proceso no realizado, excepcion";
                return BadRequest(data);
            }


        }


        [HttpPut]
        //[Authorize]
        [Produces("application/json")]
        [Route("updatePersona")]
        public IActionResult actualizarCliente([FromBody] Persona persona)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(_connectionString))
                {
                    dynamic data; //sirve para devolver una respuesta
                    using (MySqlCommand cmd = new MySqlCommand("sp_persona", conn))
                    {


                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@id_persona", persona.id);
                        cmd.Parameters.AddWithValue("@nombre", persona.nombre);
                        cmd.Parameters.AddWithValue("@apellido", persona.apellido);
                        cmd.Parameters.AddWithValue("@edad", persona.edad);
                        cmd.Parameters.AddWithValue("@Opcion", 2);

                        conn.Open();
                        DataSet setter = new DataSet();
                        MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
                        adapter.Fill(setter, "OK");


                        if (setter.Tables["OK"].Rows.Count > 0 || setter != null)
                        {
                            data = new JObject();
                            data.response = 4;
                            data.message = setter.Tables[0].Rows[0][0];
                            return Ok(data);
                        }
                        else
                        {
                            data = new JObject();
                            data.value = 0;
                            data.response = 4;
                            data.message = setter.Tables[0].Rows[0][0];
                            return BadRequest(data);
                        }
                    }

                }

            }
            catch (Exception ex)
            {
                dynamic data = new JObject();
                data.value = ex.ToString();
                data.response = 6;
                data.message = "Proceso no realizado, excepcion";
                return BadRequest(data);
            }
        }






       
    }
}
