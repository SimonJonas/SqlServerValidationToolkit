using SqlServerValidationToolkit.Model.Context;
using SqlServerValidationToolkit.Model.Entities.Rule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlServerValidationToolkit.Configurator.Controls.ValidationRules
{
    public class CustomQueryRuleEditViewViewModel : ValidationRuleEditViewViewModel
    {
        public CustomQueryRuleEditViewViewModel() : base(null) { }

        private CustomQueryRule _rule;

        public CustomQueryRuleEditViewViewModel(CustomQueryRule r, SqlServerValidationToolkitContext ctx)
            : base(r,ctx)
        {
            //TODO: Use own context-class
            _rule = r;
        }

        public string CustomQuery
        {
            get
            {
                return _rule.CustomQuery;
            }
            set
            {
                _rule.CustomQuery = value;
                RaisePropertyChanged(() => CustomQuery);
                RaisePropertyChanged(() => Header);
                RecompileQuery();
            }
        }



    }
}
