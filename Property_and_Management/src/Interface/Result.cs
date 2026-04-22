using System;

namespace Property_and_Management.Src.Interface
{
    public sealed class Result<TSuccess, TError>
    {
        private readonly TSuccess successPayloadValue;
        private readonly TError failureErrorValue;

        private Result(bool isSuccess, TSuccess successPayloadValue, TError failureErrorValue)
        {
            IsSuccess = isSuccess;
            this.successPayloadValue = successPayloadValue;
            this.failureErrorValue = failureErrorValue;
        }

        public bool IsSuccess { get; }

        public TSuccess Value
        {
            get
            {
                if (!IsSuccess)
                {
                    throw new InvalidOperationException("Cannot read Value on a failed Result.");
                }

                return successPayloadValue;
            }
        }

        public TError Error
        {
            get
            {
                if (IsSuccess)
                {
                    throw new InvalidOperationException("Cannot read Error on a successful Result.");
                }

                return failureErrorValue;
            }
        }

        public static Result<TSuccess, TError> Success(TSuccess successPayload)
        {
            return new Result<TSuccess, TError>(true, successPayload, default!);
        }

        public static Result<TSuccess, TError> Failure(TError failureError)
        {
            return new Result<TSuccess, TError>(false, default!, failureError);
        }
    }
}