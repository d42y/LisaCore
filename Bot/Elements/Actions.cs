using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LisaCore.Bot.Elements
{
    /*
     * <category>
          <pattern>FIND EMPLOYEES BY DATE *</pattern>
          <template>
            Here's a list of employees who joined on the specified date.
            <Action ID="find.by.date.action" />
          </template>
        </category>

     */

    internal class Actions
    {
        public Actions()
        {
            LoadActions();
        }

        private Dictionary<string, Action<Context, Response>> _actionMethods;

        // 2. Create a dictionary to store action methods
        private void LoadActions()
        {
            _actionMethods = new Dictionary<string, Action<Context, Response>>();

            var methods = GetType().GetMethods()
                .Where(m => m.GetCustomAttributes(typeof(ActionAttribute), false).Length > 0)
                .ToArray();

            foreach (var method in methods)
            {
                var attribute = (ActionAttribute)method.GetCustomAttributes(typeof(ActionAttribute), false).First();
                var action = (Action<Context, Response>)Delegate.CreateDelegate(typeof(Action<Context, Response>), this, method);
                _actionMethods.Add(attribute.ID, action);
            }
        }

        public void ExecuteAction(string actionId, Context context, Response result)
        {
            if (_actionMethods.ContainsKey(actionId))
            {
                _actionMethods[actionId].Invoke(context, result);
            }
        }

        [Action(ID = "find.by.date.action")]
        public void FindEmployeesByDate(Context context, Response result)
        {
            // Do something here
        }
    }
}
