
namespace GameModel
{
    public class error_base : FormatableObject
    {
        private string _errorCode = string.Empty;
        private string _error;
        private string _errorDetail;

        public string errorCode
        {
            get
            {
                return _errorCode;
            }
            set
            {
                _errorCode = value;
            }
        }

        public string error
        {
            get
            {
                return _error;
            }
            set
            {
                _error = value;
            }
        }

        public string errorDetail
        {
            get
            {
                return _errorDetail;
            }
            set
            {
                _errorDetail = value;
            }
        }
    }
}
