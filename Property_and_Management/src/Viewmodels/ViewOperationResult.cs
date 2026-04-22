namespace Property_and_Management.Src.Viewmodels
{
    public sealed class ViewOperationResult
    {
        private ViewOperationResult(bool isSuccess, string dialogTitle, string dialogMessage)
        {
            IsSuccess = isSuccess;
            DialogTitle = dialogTitle ?? string.Empty;
            DialogMessage = dialogMessage ?? string.Empty;
        }

        public bool IsSuccess { get; }
        public string DialogTitle { get; }
        public string DialogMessage { get; }

        public static ViewOperationResult Success(string dialogTitle = "", string dialogMessage = "")
        {
            return new ViewOperationResult(true, dialogTitle, dialogMessage);
        }

        public static ViewOperationResult Failure(string dialogTitle, string dialogMessage)
        {
            return new ViewOperationResult(false, dialogTitle, dialogMessage);
        }
    }
}