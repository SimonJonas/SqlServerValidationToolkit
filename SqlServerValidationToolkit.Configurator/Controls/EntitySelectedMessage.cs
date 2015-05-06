using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlServerValidationToolkit.Configurator.Controls
{
    class EntitySelectedMessage
    {
        public EntitySelectedMessage(object entity)
        {
            SelectedEntity = entity;
        }

        public object SelectedEntity
        { get; set; }
    }
}
