using System;

namespace ElTesoroDeMongli.API.Models
{
    [Serializable]
    public class ApiResponse
    {
        public int error_code;

        public bool IsSuccess => error_code == 0;
    }
}
