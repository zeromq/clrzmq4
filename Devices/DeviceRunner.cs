namespace ZeroMQ.Devices
{
    using System;

    internal class DeviceRunner
    {
        protected readonly ZDevice Device;

        public DeviceRunner(ZDevice device)
        {
            Device = device;
        }

        public virtual void Start()
        {
            Device.Run();
        }

        public virtual void Join()
        {
            Device.DoneEvent.WaitOne();
        }

        public virtual bool Join(TimeSpan timeout)
        {
            return Device.DoneEvent.WaitOne(timeout);
        }
    }
}
