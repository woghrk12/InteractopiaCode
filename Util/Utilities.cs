using System.Text;  
using System.Security.Cryptography;
using System.Collections.Generic;
using UnityEngine;

public class Utilities
{
    #region Variables

    public static readonly WaitForFixedUpdate WaitForFixedUpdate = new WaitForFixedUpdate();
    private static readonly Dictionary<float, WaitForSeconds> waitForSeconds = new Dictionary<float, WaitForSeconds>();

    #endregion Variables

    #region Methods

    public static T GetOrAddComponent<T>(GameObject gameObject) where T : Component
    {
        T component = gameObject.GetComponent<T>();

        if (component == null)
            component = gameObject.AddComponent<T>();
        
        return component;
    }

    public static WaitForSeconds WaitForSeconds(float p_sec)
    {
        if (waitForSeconds.ContainsKey(p_sec)) return waitForSeconds[p_sec];
        WaitForSeconds t_wfs = new WaitForSeconds(p_sec);
        waitForSeconds.Add(p_sec, t_wfs);
        return t_wfs;
    }

    public static string ComputeMD5(string seed, int length)
    {
        StringBuilder md5Str = new();
        byte[] byteArr = Encoding.ASCII.GetBytes(seed);
        byte[] resultArr = (new MD5CryptoServiceProvider()).ComputeHash(byteArr);

        for (int idx = 0; idx < length; idx++) { md5Str.Append(resultArr[idx].ToString("X2")); }

        return md5Str.ToString();
    }

    public string MD5HashFunc(string str)
    {
        StringBuilder MD5Str = new StringBuilder();
        byte[] byteArr = Encoding.ASCII.GetBytes(str);
        byte[] resultArr = (new MD5CryptoServiceProvider()).ComputeHash(byteArr);

        //for (int cnti = 1; cnti < resultArr.Length; cnti++) (2010.06.27)
        for (int cnti = 0; cnti < resultArr.Length; cnti++)
        {
            MD5Str.Append(resultArr[cnti].ToString("X2"));
        }
        return MD5Str.ToString();
    }

    public static Vector3 GetVectorFromAngle(float angle)
    {
        if (angle < 0)
        {
            angle += 360;
        }
        else if (angle > 360)
        {
            angle -= 360;
        }

        float angleRad = angle * (Mathf.PI / 180f);

        return new Vector3(Mathf.Cos(angleRad), Mathf.Sin(angleRad));
    }

    public static float GetAngleFromVector(Vector2 direction)
    {
        direction = direction.normalized;
        float degree = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        return degree < 0 ? degree + 360 : degree;
    }

    #endregion Methods
}
