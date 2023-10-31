using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Entity
{
    public class Window : UnityEntity
    {
        public Window(int? id, string layerName, string objectType, List<string> coordinates) : base(id, layerName, objectType, coordinates)
        {
        }

        public Window(string typeOfUnityEntity) : base(typeOfUnityEntity)
        {
        }
    }
}
