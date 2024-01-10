using Noesis;
using System;
using System.ComponentModel;

public class MainWindowViewModel : UnityEngine.MonoBehaviour, INotifyPropertyChanged
{
    public UnityEngine.Color _topColor;
    public Color TopColor { get => FromUnityColor(_topColor); }

    public UnityEngine.Color _bottomColor;
    public Color BottomColor { get => FromUnityColor(_bottomColor); }

    public NoesisEventCommand _buttonClicked;

    public NoesisEventCommand ButtonClicked { get => _buttonClicked; }

    private void Reset()
    {
        _topColor = new UnityEngine.Color(0.067f, 0.400f, 0.616f);
        _bottomColor = new UnityEngine.Color(0.071f, 0.224f, 0.341f);
    }

    void Start()
    {
        NoesisView view = GetComponent<NoesisView>();
        view.Content.DataContext = this;
    }

    public void OnButtonClicked(object parameter)
    {
        UnityEngine.Debug.Log("Button clicked");
    }

    private Color FromUnityColor(UnityEngine.Color color)
    {
        return new Color
        {
            R = (byte)(color.r * 255.0f),
            G = (byte)(color.g * 255.0f),
            B = (byte)(color.b * 255.0f),
            A = (byte)(color.a * 255.0f),
        };
    }

    private void OnValidate()
    {
        OnPropertyChanged("TopColor");
        OnPropertyChanged("BottomColor");
    }

    #region INotifyPropertyChanged implementation
    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    #endregion
}
