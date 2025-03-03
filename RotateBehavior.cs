namespace agendamento
{
    public class RotateBehavior : Behavior<Image>
    {
        private bool _isRotating;
        private Image _image;

        protected override void OnAttachedTo(Image image)
        {
            base.OnAttachedTo(image);
            _image = image;
            StartRotation();
        }

        protected override void OnDetachingFrom(Image image)
        {
            base.OnDetachingFrom(image);
            _isRotating = false;
            _image = null;
        }

        private async void StartRotation()
        {
            _isRotating = true;
            while (_isRotating && _image != null)
            {
                await _image.RotateTo(360, 2000, Easing.Linear);
                _image.Rotation = 0;
            }
        }
    }
}