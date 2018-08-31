using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace Util
{
    public class ApiCondition
    {
        public string Nombre { get; set; }
        public string Operador { get; set; }
        public string Valor { get; set; }
    }

    public static class ApiConditionExtensions{


    }
    public class apiParser<T> where T : class
    {
        private string _requesturi;
        private Type currentType = typeof(T);

        public apiParser(string requestUri)
        {
            this._requesturi = requestUri;
        }

        public List<ApiCondition> toApiConditions()
        {
            Regex regex = new Regex(@"(\w+)\[(\w*)\]");
            NameValueCollection namecol = HttpUtility.ParseQueryString(_requesturi);

            List<ApiCondition> condiciones = new List<ApiCondition>();
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

                condiciones.Add(new ApiCondition
                {
                    Nombre = match.Groups[1].Value,
                    Operador = match.Groups[2].Value,
                    Valor = namecol[key]
                });
            }
            return condiciones;
        }

        public List<Func<T, bool>> toDelegates(List<ApiCondition> conditions)
        {
            List<Func<T, bool>> validaciones = new List<Func<T, bool>>();

            foreach (PropertyInfo p in currentType.GetProperties())
            {
                ApiCondition cond = conditions.Where(con => con.Nombre.ToLower() == p.Name.ToLower()).FirstOrDefault();
                if (cond == null) continue;

                if (cond.Operador == ApiOperator.NOTNULL.Name)
                {
                    validaciones.Add((s) => p.GetValue(s) != null);
                    continue;
                }
                if (cond.Operador == ApiOperator.NULL.Name)
                {
                    validaciones.Add((s) => {
                        var prueba = p.GetValue(s);
                        return p.GetValue(s) == null;
                    });
                    continue;
                }
                if (cond.Operador == ApiOperator.GREATEROREQUAL.Name && p.PropertyType == typeof(int))
                {
                    int conditionValue = 0;
                    if (int.TryParse(cond.Valor, out conditionValue))
                    {
                        validaciones.Add((s) => {return (int)p.GetValue(s) >= conditionValue;});
                    }
                    else
                    {
                        continue;
                    }
                }

                if(cond.Operador == ApiOperator.GREATEROREQUAL.Name && p.PropertyType == typeof(int?))
                {
                    validaciones.Add((s) => {
                        int valorPropiedad = 0;
                        int valorCondicion = 0;
                        if (int.TryParse(cond.Valor, out valorCondicion) && int.TryParse(p.GetValue(s).ToString(), out valorPropiedad))
                        {
                            return valorPropiedad >= valorCondicion;
                        }
                        return false;
                    });
                }

                if(cond.Operador == ApiOperator.GREATEROREQUAL.Name && p.PropertyType == typeof(DateTime))
                {
                    DateTime date = new DateTime();
                    if (DateTime.TryParse(cond.Valor, out date))
                    {
                        validaciones.Add((s) => (DateTime)p.GetValue(s) >= date);
                    }
                    continue;
                }

                if(cond.Operador == ApiOperator.GREATEROREQUAL.Name && p.PropertyType == typeof(DateTime?)){
                    validaciones.Add((s) =>
                    {
                        DateTime valorPropiedad = new DateTime();
                        DateTime valorCondicion = new DateTime();
                        if(DateTime.TryParse(cond.Valor, out valorCondicion) && DateTime.TryParse(p.GetValue(s).ToString(), out valorPropiedad)) {
                            return valorPropiedad >= valorCondicion;
                        }
                        return false;
                    });
                }

                if(cond.Operador == ApiOperator.GREATEROREQUAL.Name && p.PropertyType == typeof(string))
                {
                    int length = 0;
                    if(int.TryParse(cond.Valor, out length))
                    {
                        validaciones.Add((s) => p.GetValue(s).ToString().Length >= length);
                    }
                    continue;
                }

                if(cond.Operador == ApiOperator)


            }
            return validaciones;
        }

        public IEnumerable<T> FilterValues(IEnumerable<T> list, List<Func<T, bool>> conditions)
        {
            foreach(Func<T, bool> cond in conditions)
            {
                list = list.Where(cond);
            }
            return list;
        }

        public sealed class ApiOperator
        {
            private readonly String name;

            public string Name
            {
                get { return name; }
            }

            public ApiOperator(string name)
            {
                this.name = name;
            }

            public static readonly ApiOperator IN = new ApiOperator("in");
            public static readonly ApiOperator NOTIN = new ApiOperator("nin");
            public static readonly ApiOperator GREATERTHAN = new ApiOperator("gt");
            public static readonly ApiOperator GREATEROREQUAL = new ApiOperator("gte");
            public static readonly ApiOperator LESSTHAN = new ApiOperator("lt");
            public static readonly ApiOperator LESSOREQUAL = new ApiOperator("lte");
            public static readonly ApiOperator EQUALS = new ApiOperator("eq");
            public static readonly ApiOperator NOTEQUALS = new ApiOperator("neq");
            public static readonly ApiOperator LIKE = new ApiOperator("lke");
            public static readonly ApiOperator NULL = new ApiOperator("empt");
            public static readonly ApiOperator NOTNULL = new ApiOperator("fill");
        }
        
    }
    
}
