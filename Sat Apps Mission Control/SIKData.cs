using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Sat_Apps_Mission_Control
{
    // Sensor data: 
    /// U - UV
    /// V - Visble
    /// I - IR
    /// X - Accel X
    /// Y - Accel Y
    /// Z - Accel Z
    /// M - Magnometer/compass angle
    /// T - SIK Temperature
    /// P - Picture from camera
    /// 
    public class SIKData
    {
        public int UV;
        public int Visible;
        public int IR;
        public int X;
        public int Y;
        public int Z;
        public int Pitch;
        public int Roll;
        public int Heading;
        public int ModifiedHeading;
        public int Temperature;
        public byte[] JPGImage;
        public DateTime MissionControlTS;

        public SIKData()
        {
            MissionControlTS = DateTime.Now;
        }
    }

    public class SIKDataViewModel : NotificationBase<SIKData>
    {
        public SIKDataViewModel(SIKData dataset = null) : base(dataset) { }

        public int UV
        {
            get { return This.UV; }
            set { SetProperty(This.UV, value, () => This.UV = value); }
        }

        public static explicit operator SIKDataViewModel(SIKData v)
        {
            SIKDataViewModel d = new SIKDataViewModel();
            d.Heading = v.Heading;
            d.IR = v.IR;
            d.JPGImage = v.JPGImage;
            d.MissionControlTS = v.MissionControlTS;
            d.ModifiedHeading = v.ModifiedHeading;
            d.Pitch = v.Pitch;
            d.Roll = v.Roll;
            d.Temperature = v.Temperature;
            d.UV = v.UV;
            d.Visible = v.Visible;
            d.X = v.X;
            d.Y = v.Y;
            d.Z = v.Z;
            return d;
        }

        public int Visible
        {
            get { return This.Visible; }
            set { SetProperty(This.Visible, value, () => This.Visible = value); }
        }
        public int IR
        {
            get { return This.IR; }
            set { SetProperty(This.IR, value, () => This.IR = value); }
        }
        public int X
        {
            get { return This.X; }
            set { SetProperty(This.X, value, () => This.X = value); }
        }
        public int Y
        {
            get { return This.Y; }
            set { SetProperty(This.Y, value, () => This.Y = value); }
        }
        public int Z
        {
            get { return This.Z; }
            set { SetProperty(This.Z, value, () => This.Z = value); }
        }
        public int Pitch
        {
            get { return This.Pitch; }
            set { SetProperty(This.Pitch, value, () => This.Pitch = value); }
        }
        public int Roll
        {
            get { return This.Roll; }
            set { SetProperty(This.Roll, value, () => This.Roll = value); }
        }
        public int Heading
        {
            get { return This.Heading; }
            set { SetProperty(This.Heading, value, () => This.Heading = value); }
        }
        public int Temperature
        {
            get { return This.Temperature; }
            set { SetProperty(This.Temperature, value, () => This.Temperature = value); }
        }
        public byte[] JPGImage
        {
            get { return This.JPGImage; }
            set { SetProperty(This.JPGImage, value, () => This.JPGImage = value); }
        }
        public int ModifiedHeading
        {
            get { return This.ModifiedHeading; }
            set { SetProperty(This.ModifiedHeading, value, () => This.ModifiedHeading = value); }
        }

        public DateTime MissionControlTS
        {
            get { return This.MissionControlTS; }
            set { SetProperty(This.MissionControlTS, value, () => This.MissionControlTS = value); }
        }
    }

    public class NotificationBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        // SetField (Name, value); // where there is a data member
        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] String property
           = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            RaisePropertyChanged(property);
            return true;
        }

        // SetField(()=> somewhere.Name = value; somewhere.Name, value) 
        // Advanced case where you rely on another property
        protected bool SetProperty<T>(T currentValue, T newValue, Action DoSet,
            [CallerMemberName] String property = null)
        {
            if (EqualityComparer<T>.Default.Equals(currentValue, newValue)) return false;
            DoSet.Invoke();
            RaisePropertyChanged(property);
            return true;
        }

        protected void RaisePropertyChanged(string property)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }
    }

    public class NotificationBase<T> : NotificationBase where T : class, new()
    {
        protected T This;

        public static implicit operator T(NotificationBase<T> thing) { return thing.This; }

        public NotificationBase(T thing = null)
        {
            This = (thing == null) ? new T() : thing;
        }
    }
}
