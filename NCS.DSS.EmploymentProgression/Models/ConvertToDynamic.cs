using System.Dynamic;

namespace NCS.DSS.EmploymentProgression.Models
{
    public class ConvertToDynamic<T> : IConvertToDynamic<T> where T : new()
    {
        public ExpandoObject RenameAndExcludeProperty(T genericObject, string oldname, string newName, string exclName)
        {
            var updatedObject = new ExpandoObject();
            foreach (var item in typeof(EmploymentProgression).GetProperties())
            {
                if (item.Name == exclName)
                    continue;
                var itemName = item.Name;
                if (itemName == oldname)
                    itemName = newName;
                AddProperty(updatedObject, itemName, item.GetValue(genericObject));
            }
            return updatedObject;
        }
        public ExpandoObject RenameProperty(T genericObject, string name, string newName)
        {
            var updatedObject = new ExpandoObject();
            foreach (var item in typeof(EmploymentProgression).GetProperties())
            {
                var itemName = item.Name;
                if (itemName == name)
                    itemName = newName;
                AddProperty(updatedObject, itemName, item.GetValue(genericObject));
            }
            return updatedObject;
        }
        public IList<ExpandoObject> RenameProperty(IList<T> objectList, string name, string newName)
        {
            var updatedObjects = new List<ExpandoObject>();
            foreach (var obj in objectList)
            {
                updatedObjects.Add(RenameProperty(obj, name, newName));
            }

            return updatedObjects;
        }
        public ExpandoObject ExcludeProperty(T genericObject, string name)
        {
            dynamic updatedObject = new ExpandoObject();
            foreach (var item in typeof(EmploymentProgression).GetProperties())
            {
                if (item.Name == name)
                    continue;
                AddProperty(updatedObject, item.Name, item.GetValue(genericObject));
            }
            return updatedObject;
        }
        public ExpandoObject ExcludeProperty(Exception exception, string[] names)
        {
            dynamic updatedObject = new ExpandoObject();
            foreach (var item in typeof(Exception).GetProperties())
            {
                if (names.Contains(item.Name))
                    continue;

                AddProperty(updatedObject, item.Name, item.GetValue(exception));
            }
            return updatedObject;
        }
        public IList<ExpandoObject> ExcludeProperty(IList<T> objectList, string name)
        {
            var updatedObjects = new List<ExpandoObject>();
            foreach (var obj in objectList)
            {
                updatedObjects.Add(ExcludeProperty(obj, name));
            }

            return updatedObjects;
        }
        public void AddProperty(ExpandoObject expando, string propertyName, object propertyValue)
        {
            var expandoDict = expando as IDictionary<string, object>;
            if (expandoDict.ContainsKey(propertyName))
                expandoDict[propertyName] = propertyValue;
            else
                expandoDict.Add(propertyName, propertyValue);
        }
    }
}
