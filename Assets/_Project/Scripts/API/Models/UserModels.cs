using System;
using System.Collections.Generic;
using UnityEngine;

namespace ElTesoroDeMongli.API.Models
{
    [Serializable]
    public class UserTransformData
    {
        public Vector3 position;
        public Quaternion rotation;
    }

    [Serializable]
    public class UserDto
    {
        public int id;
        public string nickname;
        public UserTransformData transform;
    }

    [Serializable]
    public class GetUsersResponse : ApiResponse
    {
        public List<UserDto> content;
    }

    [Serializable]
    public class UpdateUsersRequest
    {
        public List<UserUpdateData> usersUpdateData = new List<UserUpdateData>();
    }

    [Serializable]
    public class UserUpdateData
    {
        public int id;
        public UserTransformData transform;
    }

    [Serializable]
    public class UpdateUsersResponse : ApiResponse
    {
    }
}
