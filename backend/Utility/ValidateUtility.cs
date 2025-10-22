using backend.Exceptions;

namespace backend.Utilities
{
    public static class ValidateUtility
    {
        public static bool ValidatePositiveId(int id)
        {
            if (id <= 0)
            {
                throw new BadRequestException($"The number {id} should be positive");
            }

            return true;
        }
    }
}