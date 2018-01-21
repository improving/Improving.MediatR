namespace Improving.MediatR
{
    using System;

    public struct DisposableAction : IDisposable
    {
        private Action _action;

        public DisposableAction(Action action)
        {
            _action = action;
        }

        public void Dismiss()
        {
            _action = null;
        }

        void IDisposable.Dispose()
        {
            if (_action != null)
                _action();
        }
    }
}
