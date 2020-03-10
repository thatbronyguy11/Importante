using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace ViridaxGameStudios.AI
{
    public class BasicAIController : Character
    {
        private GameObject Target;
        private bool enemyFound = false;
        private float m_HitPoints;
        private int patrolCount = 0;
        private bool pointReached = false;
        private GameObject patrolTarget;
        public GameObject headLookTarget;

        #region Main Methods

        public override void Start()
        {
            base.Start();
            Animator = GetComponent<Animator>();
            m_HitPoints = HitPoints;
        }
        // Update is called once per frame
        public override void Update()
        {
            base.Update();
            
            if (CURRENT_STATE != CharacterStates.STATE_DAME && CURRENT_STATE != CharacterStates.STATE_DEAD )
            {
                
                switch (CURRENT_STATE)
                {
                    case CharacterStates.STATE_FOLLOW:
                        FollowTarget();
                        break;
                    case CharacterStates.STATE_PATROL:
                        Patrol();
                        break;
                    case CharacterStates.STATE_IDLE:
                        ResetToIDLE();
                        break;
                    default:
                        break;
                }
                ScanForObjects(gameObject.transform.position, m_DetectionRadius);


            }
            

        }
        private void ResetToIDLE()
        {
            Animator.SetBool("isWalking", false);
            Animator.SetBool("isRunning", false);
            Animator.SetBool("isAttacking", false);
        }
        #endregion
        private void OnAnimatorIK(int layerIndex)
        {
            if(enableHeadLook && headLookTarget != null)
            {
                Animator.SetLookAtPosition(headLookTarget.transform.position);
                Animator.SetLookAtWeight(headLookIntensity);
            }
            
        }
        #region Override Methods
        public override void ReceiveDamage(float damage)
        {
            base.ReceiveDamage(damage);
        }
        public override void ResumeFromDamage()
        {
            base.ResumeFromDamage();
        }
        public override void CharacterDead()
        {
            base.CharacterDead();
        }
        #endregion

        #region Helper Methods



        public void Attack()
        {
            //
            //Method Name : void Attack()
            //Purpose     : This method is called by the attack animation event. Deals the required damage to all targets in range..
            //Re-use      : none
            //Input       : none
            //Output      : none
            //
            if (CURRENT_STATE != CharacterStates.STATE_ATTACK && CURRENT_STATE != CharacterStates.STATE_PATROL)
            {
                Animator.SetBool("isAttacking", false);
                if (CURRENT_STATE != CharacterStates.STATE_PATROL)
                {
                    CURRENT_STATE = CharacterStates.STATE_IDLE;
                }
                return;

            }
            //Ray ray = new Ray();
            //ray = Camera.main.ScreenPointToRay(transform.forward);
            RaycastHit[] hits;
            hits = Physics.SphereCastAll(transform.position, m_AttackRange, transform.forward);
            foreach (RaycastHit hit in hits)
            {
                float angle = Vector3.Angle(hit.transform.position - transform.position, transform.forward);
                if(hit.transform.gameObject.tag == "Player")
                {
                    float distance = Vector3.Distance(transform.position, Target.transform.position);
                    if (angle <= m_DamageAngle / 2 && distance <= m_AttackRange)
                    {
                        hit.transform.gameObject.SendMessage("ReceiveDamage", Damage);
                    }
                }
            }
            if(CURRENT_STATE != CharacterStates.STATE_PATROL)
            {
                CURRENT_STATE = CharacterStates.STATE_IDLE;
            }
            
        }
        
        protected void Patrol()
        {
            //
            //Method Name : void Patrol()
            //Purpose     : This method allows the character to progress towards the patrol points.
            //Re-use      : none
            //Input       : none
            //Output      : none
            //
            if (pointReached)
            {
                pointReached = false;
                if (patrolInOrder)
                {
                    if (patrolCount < patrolPoints.Count - 1)
                    {
                        patrolCount++;
                    }
                    else
                    {
                        patrolCount = 0;
                    }
                }
                else
                {
                    Random rnd = new Random();
                    patrolCount = Random.Range(0, patrolPoints.Count);
                }
            }
            else
            {
                if(Target == null)
                {
                    Animator.SetBool("isRunning", false);
                    Animator.SetBool("isWalking", true);
                    Animator.SetBool("isAttacking", false);
                    patrolTarget = patrolPoints[patrolCount];
                    LookAt(patrolTarget);
                    transform.position += transform.forward * MovementSpeed * Time.deltaTime;
                }
                else
                {
                    Animator.SetBool("isWalking", false);
                    PatrolFollow();
                }
            }
            
        }
        private void PatrolFollow()
        {
            //
            //Method Name : void PatrolFollow()
            //Purpose     : This method allows the character to follow an enemy,
            //              and then return to patroling once the enemy is dead or out of range.
            //Re-use      : none
            //Input       : none
            //Output      : none
            //
            if (Target != null)
            {
                float distance = Vector3.Distance(transform.position, Target.transform.position);
                if (distance <= m_AttackRange)
                {
                    LookAt(Target);
                    if(canAttack)
                    {
                        Animator.SetBool("isRunning", false);
                        Animator.SetBool("isAttacking", true);
                    }
                }
                else
                {
                    transform.position += transform.forward * MovementSpeed * Time.deltaTime;
                    Animator.SetBool("isRunning", true);
                    Animator.SetBool("isAttacking", false);
                }
            }
        }
        protected void FollowTarget()
        {
            //
            //Method Name : void FollowTarget()
            //Purpose     : This method moves the character to where the target position is. In most casess, the player position.
            //Re-use      : none
            //Input       : none
            //Output      : none
            //
            if (Target != null)
            {
                float distance = Vector3.Distance(transform.position, Target.transform.position);
                if (distance <= m_AttackRange)
                {
                    if(canAttack)
                    {
                        CURRENT_STATE = CharacterStates.STATE_ATTACK;

                        Animator.SetBool("isRunning", false);
                        Animator.SetBool("isAttacking", true);
                        LookAt(Target);
                    }
                    

                }
                else
                {
                    transform.position += transform.forward * MovementSpeed * Time.deltaTime;
                    Animator.SetBool("isRunning", true);
                    Animator.SetBool("isAttacking", false);
                }
            }
        }

        private void ScanForObjects(Vector3 center, float radius)
        {
            //
            //Method Name : void ScanForObjects(Vector3 center, float radius) 
            //Purpose     : This method uses the Physics.OverlapSphere method to scan for objects within a given radius.
            //Re-use      : none
            //Input       : Vector3 center, float radius
            //Output      : none
            //
            Collider[] hitColliders = Physics.OverlapSphere(center, radius);
            int i = 0;
            enemyFound = false;
            while (i < hitColliders.Length)
            {
                //Filter out all GameObjects to get only those that are enemies.
                if (enemyTags.Contains(hitColliders[i].tag))
                {
                    Target = hitColliders[i].gameObject;
                    LookAt(Target);
                    if (CURRENT_STATE == CharacterStates.STATE_IDLE && CURRENT_STATE != CharacterStates.STATE_PATROL)
                    {
                        CURRENT_STATE = CharacterStates.STATE_FOLLOW;
                    }
                    
                    enemyFound = true;
                    
                }
                i++;
            }
            if (!enemyFound)
            {
                Target = null;
                if (CURRENT_STATE != CharacterStates.STATE_PATROL)
                {
                    CURRENT_STATE = CharacterStates.STATE_IDLE;
                    Animator.SetBool("isRunning", false);
                    Animator.SetBool("isWalking", false);
                    Animator.SetBool("isAttacking", false);
                }
            }
        }
        protected void LookAt(GameObject Target)
        {
            Quaternion targetRotation = Quaternion.LookRotation(Target.transform.position - transform.position);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 12 * Time.deltaTime);
        }
        #endregion
        void OnCollisionEnter(Collision collision)
        {
            if (CURRENT_STATE == CharacterStates.STATE_PATROL)
            {
                if (collision.gameObject.tag == "PatrolPoint" && collision.gameObject.name == patrolTarget.gameObject.name)
                {
                    pointReached = true;
                }
            }
            

        }
    }
    
    
}

