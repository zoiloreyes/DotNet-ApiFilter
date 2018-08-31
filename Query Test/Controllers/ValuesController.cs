using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using Query_Test.Model;
using Util;

namespace Query_Test.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private List<Person> Personas = new List<Person>();
        public ValuesController()
        {
            this.Personas = new List<Person>()
            {
                new Person(){Name = "Juan",
                             BirthDate = new DateTime(1998,04,03),
                             DeceaseDate = null,
                             DiaperMoney = null,
                             NumberOfKids = 1,
                             NumberOfLimbs = 1},
                 new Person(){Name = "Maria",
                             BirthDate = new DateTime(2005,04,03),
                             DeceaseDate = null,
                             DiaperMoney = null,
                             NumberOfKids = 5,
                             NumberOfLimbs = 2},
                  new Person(){Name = "Nicolas",
                             BirthDate = new DateTime(2008,04,03),
                             DeceaseDate = null,
                             DiaperMoney = null,
                             NumberOfKids = 2,
                             NumberOfLimbs = 3},
                   new Person(){Name = "Pedro",
                             BirthDate = new DateTime(2010,04,03),
                             DeceaseDate = null,
                             DiaperMoney = null,
                             NumberOfKids = null,
                             NumberOfLimbs = 4}
            };
        }

        // GET api/values
        [HttpGet]
        public ActionResult<IEnumerable<Person>> Get()
        {
            var queryString = Request.QueryString.ToString();
            if (!String.IsNullOrWhiteSpace(queryString))
            {
                apiParser<Person> parser = new apiParser<Person>(queryString);
                var conditions = parser.toApiConditions();
                var delegates = parser.toDelegates(conditions);
                Personas = parser.FilterValues(this.Personas, delegates).ToList();
            }


            return Personas;
        }


        public sealed class SqlOperator
        {
            private readonly String name;
            private readonly String value;
            public static List<SqlOperator> ValidOperators = new List<SqlOperator>()
            {
                IN,
                NOTIN,
                GREATERTHAN,
                GREATEROREQUAL,
                LESSTHAN,
                EQUALS,
                NOTEQUALS,
                LIKE,
                NOTNULL
            };

            public SqlOperator(string name, string value)
            {
                this.name = name;
                this.value = value;
            }
            public static readonly SqlOperator IN = new SqlOperator("in", "IN");
            public static readonly SqlOperator NOTIN = new SqlOperator("nin", "NOT IN");
            public static readonly SqlOperator GREATERTHAN = new SqlOperator("gt", ">");
            public static readonly SqlOperator GREATEROREQUAL = new SqlOperator("gte", ">=");
            public static readonly SqlOperator LESSTHAN = new SqlOperator("lt", "<");
            public static readonly SqlOperator LESSOREQUAL = new SqlOperator("lte", "<=");
            public static readonly SqlOperator EQUALS = new SqlOperator("eq", "=");
            public static readonly SqlOperator NOTEQUALS = new SqlOperator("neq", "=");
            public static readonly SqlOperator LIKE = new SqlOperator("lke", "LIKE");
            public static readonly SqlOperator NULL = new SqlOperator("empt", "IS NULL");
            public static readonly SqlOperator NOTNULL = new SqlOperator("in", "IS NOT NULL");

            public static bool isValid(string operatorString)
            {
                return true;
                // return availableOperators.Any(op => op.name == operatorString);
            }
        }


        public void Calculareichon(string querystring)
        {
            Regex regex = new Regex(@"(\w+)\[(\w*)\]");
            NameValueCollection namecol = HttpUtility.ParseQueryString(querystring);
            foreach (string key in namecol.Keys)
            {
                if (key == null)
                {
                    continue;
                }
                Match match = regex.Match(key);
                if (!match.Success)
                {
                    continue;
                }

                var propiedad = match.Groups[1].Value;
                var operador = match.Groups[2].Value;
                var valor = namecol[key];



                //var col = namecol[key];
                //Console.WriteLine("Rah");
            }
        }

        public class InvalidApiOperatorException : Exception
        {
            public InvalidApiOperatorException()
            {

            }

            public InvalidApiOperatorException(string message) : base(message)
            {

            }

            public InvalidApiOperatorException(string message, Exception inner) : base(message, inner)
            {

            }
        }

        public static class ApiOperators
        {
            public const string IN = "in";
            public const string NOTIN = "nin";
            public const string GREATERTHAN = "gt";
            public const string GREATEROREQUAL = "gte";
            public const string LESSTHAN = "lt";
            public const string LESSOREQUAL = "lte";
            public const string EQUALS = "eq";
            public const string NOTEQUALS = "neq";
            public const string LIKE = "lke";
            public const string NULL = "empt";
            public const string NOTNULL = "fill";
            public static string ToSqlOperator(string apiOperator)
            {
                switch (apiOperator)
                {
                    case IN:
                        return "IN";
                    case NOTIN:
                        return "NOT IN";
                    case GREATERTHAN:
                        return ">";
                    case GREATEROREQUAL:
                        return ">=";
                    case LESSTHAN:
                        return "<";
                    case LESSOREQUAL:
                        return "<=";
                    case EQUALS:
                        return "=";
                    case NOTEQUALS:
                        return "!=";
                    case LIKE:
                        return "LIKE";
                    case NULL:
                        return "IS NULL";
                    case NOTNULL:
                        return "IS NOT NULL";
                    default:
                        throw new InvalidApiOperatorException($"Operator {apiOperator} is invalid");
                }
            }
        }

        public class ApiFilter
        {

        }
        public static class ApiValue
        {
            public static Regex arrayRegex = new Regex(@"\[((.*))\]");
            public static string ToSqlValue(string apiValue)
            {
                Match match;

                match = arrayRegex.Match(apiValue);
                if (match.Success)
                {
                    return $"[{match.Groups[1].Value.Split(",")}]";
                }
                return "";
            }
        }
        public class Condition
        {
            Dictionary<string, string> SQLOperators = new Dictionary<string, string>()
            {
                { "in", "IN" },
                { "nin", "NOT IN" },
                { "gt", ">" },
                { "gte", ">=" },
                { "lt", "<" },
                { "lte", "<=" },
                { "eq", "=" },
                { "neq", "!=" },
                { "lke", "LIKE" },
                { "empt", "IS NULL" },
                { "fill", "IS NOT NULL" },
            };
            public string Property { get; set; }
            public string Operator { get; set; }
            public string Value { get; set; }

            // GET api/values/5
            [HttpGet("{id}")]
            public ActionResult<string> Get(int id)
            {
                return "value";
            }

            // POST api/values
            [HttpPost]
            public void Post([FromBody] string value)
            {
            }

            // PUT api/values/5
            [HttpPut("{id}")]
            public void Put(int id, [FromBody] string value)
            {
            }

            // DELETE api/values/5
            [HttpDelete("{id}")]
            public void Delete(int id)
            {
            }
        }
    }
}