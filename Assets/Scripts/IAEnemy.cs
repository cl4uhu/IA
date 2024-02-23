using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class IAEnemy : MonoBehaviour
{
    // Para crear constantes numéricas pero escritas. Patrolling = 0, en vez de visualizar un 0, veremos el nombre.
   public enum State
    {
        Patrolling, 
        Chasing,
        Waiting,
        Attacking
    }
    
    //Estado actual.
    public State currentState;

    //Dar información para ir a x sitio.
    NavMeshAgent enemyAgent; 
    
    //Almacenamiento posición del jugador, pàra saber que es lo que tiene que perseguir la IA.
    Transform playerTransform; 

    //Tamaño del area.
    [SerializeField] Vector2 patrolAreaSize; 

    //Detectar si nuestro jugador está en rago.
    [SerializeField] float visionRange = 15; 

    //Controla el rango de visión de nuestra IA. 
    [SerializeField] float visionAngle = 90; 

    //Detectar si el jugador se ha movido o no desde la última vez que lo ha visto. 
    Vector3 lastTargetPosition;

    float searchTimer;
    //Tiempo que está patrullando por esa zona cuando deje de ver al jugador. 
    [SerializeField] float searchWaitTime = 15;
    //Zona de investigación creada cuando deje de ver al jugador.
    [SerializeField] float searchRadius = 30;

    //Almacenará todas las bases de patrulla.
    [SerializeField] Transform[] patrolBases;
    //Velocidad IA al dirigirse a la base.
    [SerializeField] float velocity = 22f;
    //Indicación a la IA.
    private int currentBaseIndex =0;
    //Tiempo de espera.
    [SerializeField] float waitingTime = 5f;
    [SerializeField] float attackRange = 2f;
    
    bool waited = true;

    //Para asignar lo anterior.
    void Awake()
    {
        enemyAgent = GetComponent<NavMeshAgent>();
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform; 
    }
    
    void Start()
    {
       //Asignación de un estado por defecto para que empiece por este, siempre que queramos poner un estado deberemos escribir anteriormente "State".

        Patter();
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

            case State.Waiting:
                Wait();
            break;

            case State.Attacking:
                Attack();
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
            //Patter();
            currentState = State.Waiting;
        }
    }

   //(Función) Comportamiento IA mientras está persiguiendo.
    void Chase()
    {
        enemyAgent.destination = playerTransform.position; 
        
         if (OnRange())
        {
          if (Vector3.Distance(transform.position, playerTransform.position) < attackRange)
            {
                currentState = State.Attacking;
            }  
        }

        //Comprobación si el rango del personaje es falso.
        if (OnRange() == false)
        {
            currentState = State.Patrolling;
        }
    }

    void Wait()
    {
        if(OnRange() == true)
            {
                currentState = State.Chasing;
            }

        if(waited == true)
        {
            StartCoroutine(DoWait());
        }
        
    }

    System.Collections.IEnumerator DoWait()
        {
            waited = false;
            yield return new WaitForSeconds(waitingTime);
            Patter();
            currentState = State.Patrolling;
            waited = true;
        }

    void Attack()
    {
        
        Debug.Log("Te he pegao");
          

        currentState = State.Chasing;
    }

    //Para hacer que el enemigo vayda de un punto a otro y luego vuelva al inicial.
    void Patter()
    {
       //Patrón de las bases.
       Transform target = patrolBases[currentBaseIndex];
       //transform.position = Vector3.MoveTowards(transform.position, target.position, velocity * Time.deltaTime);
       enemyAgent.destination = patrolBases[currentBaseIndex].position; 

        if (Vector3.Distance(transform.position, target.position) < 1f)
        {
            //currentState = State.Waiting;
            currentBaseIndex = (currentBaseIndex + 1) % patrolBases.Length;

            if (currentBaseIndex == 3)
            {
                currentBaseIndex = 0;
            }
        }
        
        if(OnRange() == true)
        {
            currentState = State.Chasing;
        }
    }

    //Comprobar si el personaje está en rango.
    bool OnRange()
    {
        
        //Dirección en la qu eestá el jugador.
        Vector3 directionToPlayer = playerTransform.position - transform.position; 
        
        //Distancia entre jugador y IA artificial.
        float distanceToPlayer = directionToPlayer.magnitude; 

        //Ángulo que hay hacia el jugador.
        float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);


        //Me comprueba si estoy dentro del rango de visión, y del angúlo del campo visual.
        if(distanceToPlayer <= visionRange && angleToPlayer < visionAngle * 0.5f)
        {
            //return true;
            
            //Detección del personaje. 
            if(playerTransform.position == lastTargetPosition)
            {
                    return true;
            }
            
            //Si el enemigo está dentro del campo de visión y ángulo del jugador, disparará un rayo hacia él. Si hay algo entremedio no. 
            RaycastHit hit;
            if(Physics.Raycast(transform.position, directionToPlayer, out hit, distanceToPlayer))
            {
                if(hit.collider.CompareTag("Player"))
                {
                    return true;
                }
            }
        }
        return false;
    }
    
    void Search()
    {
        //Si el jugador está dentro de la zona que el enmigo empiece a perseguirlo.
        if (OnRange() == true)
        {
            currentState = State.Chasing;
        }

        //Cronómetro.
        searchTimer += Time.deltaTime;

        //El enemigo se dirige a la zona en la que se vió por última vez al jugador.
        if(searchTimer < searchWaitTime)
        {
            if(enemyAgent.remainingDistance < 0.5f)
            { 
            Vector3 randomSearchPoint = lastTargetPosition + Random.insideUnitSphere * searchRadius;
            randomSearchPoint.y = lastTargetPosition.y;
            enemyAgent.destination = randomSearchPoint;
            }
        }
        //Si no ocurre el enemigo directamente pasará al modo patrullar.
        else
        {
            currentState = State.Patrolling;
        }
    }
}
