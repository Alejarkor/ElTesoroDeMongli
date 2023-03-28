using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text.RegularExpressions;


public class LoginController : MonoBehaviour
{
    [SerializeField]
    private ApiCaller apiCaller = null;

    [SerializeField]
    private TMP_Text mailTextField, passTextField = null;

    [SerializeField]
    public TextMeshProUGUI alertText = null;
    [SerializeField]
    public float visibleTime = 3;

    void Start()
    {
        alertText.gameObject.SetActive(false);
    }

    private void ViewMessage(string message)
    {

        alertText.text = message;
        alertText.gameObject.SetActive(true);
        StartCoroutine(HideAlert());
    }

    private IEnumerator HideAlert()
    {
        yield return new WaitForSeconds(visibleTime);
        alertText.gameObject.SetActive(false);
    }

    private bool ValidateMailField(string mail)
    {
        // Expresión regular para verificar si el string es un correo electrónico válido
        string patronMail = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";

        if (Regex.IsMatch(mail, patronMail))
        {
            return true;
        }
        else
        {
            return false;
        }



    }

 

    public void SendRequest()
    {
        if (ValidateMailField(mailTextField.text.Trim()))
        {
            apiCaller.LoginRequest(mailTextField.text.Trim(), passTextField.text.Trim());
        }
         else
        {
            ViewMessage("El correo electrónico no es válido");
        }
    }
}
