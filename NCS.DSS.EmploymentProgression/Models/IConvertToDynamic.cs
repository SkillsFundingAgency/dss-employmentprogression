using System;
using System.Collections.Generic;
using System.Dynamic;

namespace NCS.DSS.EmploymentProgression.Models
{
    public interface IConvertToDynamic<T> where T : new()
    {
        public ExpandoObject RenameAndExcludeProperty(T genericObject, string oldname, string newName, string exclName);

        public ExpandoObject RenameProperty(T genericObject, string name, string newName);

        public IList<ExpandoObject> RenameProperty(IList<T> objectList, string name, string newName);

        public ExpandoObject ExcludeProperty(T genericObject, string name);

        public IList<ExpandoObject> ExcludeProperty(IList<T> objectList, string name);

        public void AddProperty(ExpandoObject expando, string propertyName, object propertyValue);
        public ExpandoObject ExcludeProperty(Exception exception, string[] names);

    }
}
