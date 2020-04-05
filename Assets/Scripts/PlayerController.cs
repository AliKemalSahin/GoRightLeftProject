using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    delegate void TurnDelegate();      //fonksiyon tutan pointer olarak geçer 
    TurnDelegate turnDelegate;


    GameManager gameManager;
    Animator anim;
    public float moveSpeed = 2;
    bool lookingRight = true;
    public Transform rayOrigin;
    public Text scoreTxt, hscoreTxt;
    public ParticleSystem effect;
    public int Score { get; private set; }
    public int HScore { get; private set; }
    // Start is called before the first frame update
    void Start()
    {
        #region PLATFORM FOR TURNING
            #if UNITY_EDITOR

                    turnDelegate = TurnPlayerUsingKeyboard;
            #endif
            #if UNITY_ANDROID

                        turnDelegate = TurnPlayerUsingTouch;
            #endif
        #endregion


        gameManager = GameObject.FindObjectOfType<GameManager>();     // sürükle bırak yapmamak için zaten GameManager tipinde tek obje var.
        anim = gameObject.GetComponent<Animator>();

        LoadHighScore();
    }
     
    private void LoadHighScore()
    {
        HScore = PlayerPrefs.GetInt("hiscore");
        hscoreTxt.text = HScore.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        if (!gameManager.gameStarted)
            return;    // oyun başlamamışsa hiç bir şey yapma

        anim.SetTrigger("gameStarted");

        moveSpeed *= 1.0001f; // zamanla player hızlansın
        //transform.position += transform.forward * Time.deltaTime * moveSpeed;
        transform.Translate(new Vector3(0, 0, 1) * Time.deltaTime * moveSpeed);   // hareket etme kodu üstekiyle aynı

        turnDelegate();   // ilgili platforma göre mobil veya pc seçim yapıcak ona göre dönüş sağlayacak

        CheckFalling();
    }

    private void TurnPlayerUsingKeyboard()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            Turn();
    }
    private void TurnPlayerUsingTouch()
    {
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)   // 0 demek tek parmak kullanıcaz  // touch began ise ekrana basmaya basladıgımızda gerçeklessin demiş olduk
            Turn();
    }

    float elapsedTime = 0;
    float freq = 1 / 5f;     // saniyede 5 kere ışın kontrolü yapılcak

    private void CheckFalling()
    {
        if((elapsedTime += Time.deltaTime) > freq)    // 0.2 oldugunda ışını çizdiriyoruz sürekli çizmesin yormasın diye.
        {
            if (!Physics.Raycast(rayOrigin.position, new Vector3(0, -1, 0)))   // eğer aşağıda bir şey yok ise
            {
                anim.SetTrigger("falling");
                gameManager.RestartGame();
                elapsedTime = 0;
            }
        }

    }

    private void Turn()
    {
        if (lookingRight)
        {
            transform.Rotate(new Vector3(0, 1, 0), -90);    // sağa bakıyorsa y eksenine -90 derece ekleyerek sola baktırdık.
        }
        else
        {
            transform.Rotate(new Vector3(0, 1, 0), 90);
        }
        lookingRight = !lookingRight;


    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag.Equals("crystal"))
        {
            var efekt = Instantiate(effect, transform);
            Destroy(efekt, 1f);
            Score++;
            scoreTxt.text = Score.ToString();
            if(Score > HScore)
            {
                HScore = Score;
                PlayerPrefs.SetInt("hiscore", HScore);
                hscoreTxt.text = HScore.ToString();
            }
            Destroy(other.gameObject);
        }
        
    }
    private void OnCollisionExit(Collision collision)   // arkada kalan küpleri yok etme
    {
        Destroy(collision.gameObject, 2f);
    }

}
