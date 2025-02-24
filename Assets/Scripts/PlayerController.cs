using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private CharacterController controller;

    Transform cam;

    [Header("Movement")]
    //variables para controlar velocidad, altura de salto y gravedad
    [SerializeField]private float speed = 5;
    [SerializeField]private float jumpHeight = 1;
    [SerializeField]private float gravity = -9.81f;
    //Vector para aplicar la gravedad
    private Vector3 playerVelocity;

    [Header("Ground Sensor")]
    //variables para el ground sensor
    [SerializeField]private bool isGrounded;
    [SerializeField]private Transform groundSensor;
    [SerializeField]private float sensorRadius = 0.1f;
    [SerializeField]private LayerMask ground;

    //variables para rotacion del personaje
    private float turnSmoothVelocity;
    private float turnSmoothTime = 0.1f;
    
    // Start is called before the first frame update
    void Awake()
    {
        //Asignamos el character controller a su variable
        controller = GetComponent<CharacterController>();
        //Asignamos la camara
        cam = Camera.main.transform;

        //Con esto podemos esconder el icono del raton para que no moleste
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        //Llamamos la funcion de movimiento
        Movement();
            
        //Lamamaos la funcion de salto
        Jump();
    }

    //Movimiento TPS con Freelook camera
    void Movement()
    {
        //Creamos un Vector3 y en los ejes X y Z le asignamos los inputs de movimiento
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        Vector3 move = new Vector3(x, 0, z).normalized;

        if(move != Vector3.zero)
        {
            //Creamos una variable float para almacenar la posicion a la que queremos que mire el personaje
            //Usamos la funcion Atan2 para calcular el angulo al que tendra que mirar nuestro personaje
            //lo multiplicamos por Rad2Deg para que nos de el valor en grados y le sumamos la rotacion de la camara en Y para que segund donde mire la camara afecte a la rotacion
            float targetAngle = Mathf.Atan2(move.x, move.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
            //Usamos un SmoothDamp para que nos haga una transicion entre el angulo actual y el de la camara
            //de esta forma no nos rotara de golpe al personaje
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, cam.eulerAngles.y, ref turnSmoothVelocity, turnSmoothTime);
            //le aplicamos la rotacion al personaje
            transform.rotation = Quaternion.Euler(0, angle, 0);

            //Creamos otro Vector3 el cual multiplicaremos el angulo al que queremos que mire el personaje por un vector hacia delante
            //para que el personaje camine en la direccion correcta a la que mira
            Vector3 moveDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            //Funcion del character controller a la que le pasamos el Vector que habiamos creado y lo multiplicamos por la velocidad para movernos
            controller.Move(moveDirection.normalized * speed * Time.deltaTime);
        }
    }

    //Funcion de salto y gravedad
    void Jump()
    {
        //Le asignamos a la boleana isGrounded su valor dependiendo del CheckSphere
        //CheckSphere crea una esfera pasandole la poscion, radio y layer con la que queremos que interactue
        //si la esfera entra en contacto con la capa que le digamos convertira nuestra boleana en true y si no entra en contacto en false
        isGrounded = Physics.CheckSphere(groundSensor.position, sensorRadius, ground);

        //Si estamos en el suelo y playervelocity es menor que 0 hacemos que le vuelva a poner el valor a 0
        //esto es para evitar que siga aplicando fuerza de gravedad cuando estemos en el suelo y evitar comportamientos extraños
        if(isGrounded && playerVelocity.y < 0)
        {
            playerVelocity.y = 0;
        }

        //si estamos en el suelo y pulasamos el imput de salto hacemos que salte el personaje
        if(isGrounded && Input.GetButtonDown("Jump"))
        {
            //Formula para hacer que los saltos sean de una altura concreta
            //la altura depende del valor de jumpHeight 
            //Si jumpHeigt es 1 saltara 1 metro de alto
            playerVelocity.y = Mathf.Sqrt(jumpHeight * -2 * gravity); 
        }

        //a playervelocity.y le iremos sumando el valor de la gravedad
        playerVelocity.y += gravity * Time.deltaTime;
        //como playervelocity en el eje Y es un valor negativo esto nos empuja al personaje hacia abajo
        //asi le aplicaremos la gravedad
        controller.Move(playerVelocity * Time.deltaTime);
    }

    //Funcion para dibujar Gizmos
    void OnDrawGizmosSelected()
    {
        //Dibujamos un gizmo para el ground sensor
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(groundSensor.position, sensorRadius);
    }
}
