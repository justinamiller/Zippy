using System;

namespace CoreConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            var obj = new Models.ComplexModelObject();
         var str=   Zippy.JSON.SerializeObjectToString(obj);
        }
    }
}
