using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class IAEnemy : MonoBehaviour
{
    // Para crear constantes numéricas pero escritas. Patrolling = 0, en vez de visualizar un 0, veremos el nombre.
    enum State
    {
        Patrolling, 
        Chasing
    }
    
    //Estado actual.
    State currentState;

    //Dar información para ir a x sitio.
    NavMeshAgent enemyAgent; 
    
    //Almacenamiento posición del jugador, pàra saber que es lo que tiene que perseguir la IA.
    Transform playerTransform; 

    // Centro de la zona donde patrullará nuestra inteligencia artificial. 
    [SerializeField] Transform patrolAreaCenter;

    //Tamaño del area.
    [SerializeField] Vector2 patrolAreaSize; 

    //Detectar si nuestro personaje está en rago.
    [SerializeField] float visionRange = 15; 

    //Controla el rango de visión de nuestra IA. 
    [SerializeField] float visionAngle = 90; 

    //Para asignar lo anterior.
    void Awake()
    {
        enemyAgent = GetComponent<NavMeshAgent>();
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform; 
    }
    
    void Start()
    {
       //Asignación de un estado por defecto para que empiece por este, siempre que queramos poner un estado deberemos escribir anteriormente "State".
       currentState = State.Patrolling;  
    }

    void Update()
    {
        //Según en el estado en el que esté nuestra IA, para llamar a una de las dos funciones. 
        switch (currentState)
        {
            case State.Patrolling:
                Patrol();
            break;
            case State.Chasing:
                Chase();
            break;
        }
    }

    //(Función) Comportamiento IA mientras está patrullando. 
    void Patrol()
    {
        //La IA comprueba si el personaje está en rango. Pasar del estado de patrullar a perseguir.
        if (OnRange() == true)
        {
            currentState = State.Chasing;
        }

    //La IA detectará un punto aleatorio de la escena, una vez llegue a ese punto buscará otro.
        if(enemyAgent.remainingDistance < 0.5f)
        {
            SetRandomPoint();
        }
    }

   //(Función) Comportamiento IA mientras está persiguiendo.
    void Chase()
    {
        enemyAgent.destination = playerTransform.position; 

        //Comprobación si el rango del personaje es falso.
        if (OnRange() == false)
        {
            currentState = State.Patrolling;
        }
    }

    //Nos buscará un punto aleatorio del area enemiga.
    void SetRandomPoint()
    {
        float randomX = Random.Range(-patrolAreaSize.x / 2, patrolAreaSize.x / 2); 
        float randomZ = Random.Range(-patrolAreaSize.y / 2, patrolAreaSize.y / 2);
        Vector3 randomPoint = new Vector3(randomX, 0f, randomZ) + patrolAreaCenter.position;

        enemyAgent.destination = randomPoint;
    }

    //Comprobar si el personaje está en rango.
    bool OnRange()
    {
        //IA SIMPLE. Posición IA/Posición personaje. Nos mide la distancia que hay entre estos dos, una vez medida la comprueba. Añadiendo el ángulo de visión.
        /*if(Vector3.Distance(transform.position, playerTransform.position) <= visionRange)
        {
            //Si estamos en rango que nos devuelva verdadero.
            return true; 
        }
        //Si no estamos en rango/fuera de alcance que nos devuelva falso.
        return false; */
        
        //Dirección en la qu eestá el jugador.
        Vector3 directionToPlayer = playerTransform.position - transform.position; 
        
        //Distancia entre jugador y IA artificial.
        float distanceToPlayer = directionToPlayer.magnitude; 

        //Ángulo que hay hacia el jugador.
        float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);


        //Me comprueba si estoy dentro del rango de visión, y del angúlo del campo visual.
        if(distanceToPlayer <= visionRange && angleToPlayer < visionAngle * 0.5f)
        {
            return true;
        }
        return false;
    }

    //Para poder ver el tamaño de esto.
    void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(patrolAreaCenter.position, new Vector3 (patrolAreaSize.x, 0, patrolAreaSize.y));

    //Para saber el rango de nuestra IA. 
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, visionRange);

    //Para saber el rango de visión
        Gizmos.color = Color.green;
        
        Vector3 fovLine1 = Quaternion.AngleAxis(visionAngle * 0.5f, transform.up) * transform.forward * visionRange; 
        Vector3 fovLine2 = Quaternion.AngleAxis(-visionAngle * 0.5f, transform.up) * transform.forward * visionRange;
        
        Gizmos.DrawLine(transform.position, transform.position + fovLine1);
        Gizmos.DrawLine(transform.position, transform.position + fovLine2);
    }
}
