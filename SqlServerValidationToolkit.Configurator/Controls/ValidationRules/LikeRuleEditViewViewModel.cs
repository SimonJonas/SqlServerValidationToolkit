using SqlServerValidationToolkit.Model.Entities.Rule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlServerValidationToolkit.Configurator.Controls.ValidationRules
{
    public class LikeRuleEditViewViewModel : ValidationRuleEditViewViewModel
    {
        public LikeRuleEditViewViewModel() : base(null) { }
        private LikeRule _rule;

        public LikeRuleEditViewViewModel(LikeRule r)
            : base(r)
        {
            _rule = r;
        }

        public string LikeExpression
        {
            get
            {
                return _rule.LikeExpression;
            }
            set
            {
                _rule.LikeExpression = value;
                RaisePropertyChanged(() => LikeExpression);
                RaisePropertyChanged(() => Header);
                RecompileQuery();
            }
        }


    }
}
