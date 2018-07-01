using UnityEngine;
using System.Collections.Generic;

namespace TrailBoids
{
    public class BoidController : MonoBehaviour
    {
        #region Editable properties
        [SerializeField] int _spawnCount = 10;
        [SerializeField] float _spawnRadius = 4;

        [SerializeField] float _velocity = 6;
        [SerializeField, Range(0, 1)] float _velocityVariance = 0.5f;
        [SerializeField] Vector3 _scroll = Vector3.zero;

        [SerializeField] float _rotationSpeed = 4;
        [SerializeField] float _neighborDistance = 2;
        #endregion

        Path path;
        Vector3 leaderPosition;
        Vector3 leaderDirection;
        float animeRate;
        //Vector3 currentDirection; // Vector3.up; //Vector3.forward;
        //Vector3 currentPosition;

        #region Boid array

        class Boid
        {
            public Vector3 position;
            public Quaternion rotation;
            public float noiseOffset;
            public GameObject gameObject;
        }

        List<Boid> _boids = new List<Boid>();

        #endregion

        #region Boid behavior

        // Calculates a separation vector from a boid with another boid.
        Vector3 GetSeparationVector(Boid self, Boid target)
        {
            var diff = target.position - self.position;
            var diffLen = diff.magnitude;
            var scaler = Mathf.Clamp01(1 - diffLen / _neighborDistance);
            return diff * scaler / diffLen;
        }

        // Reynolds' steering behavior
        void SteerBoid(Boid self)
        {
            //velocityを変更
            Vector3 v = this.leaderDirection * -9.0f; //this.path.GetCurrentVelocity();
            ParticleSystem.VelocityOverLifetimeModule velocity = self.gameObject.GetComponent<ParticleSystem>().velocityOverLifetime;
            velocity.x = v.x; velocity.y = v.y; velocity.z = v.z;
            var particle = self.gameObject.GetComponent<ParticleSystem>();
            /*
            Debug.Log("particle.x: " + particle.velocityOverLifetime.xMultiplier);
            Debug.Log("particle.y: " + particle.velocityOverLifetime.yMultiplier);
            Debug.Log("particle.z: " + particle.velocityOverLifetime.zMultiplier);
            */

            // Steering vectors
            var separation = Vector3.zero;
            //var alignment = transform.up;
            var alignment = this.leaderDirection;
            //var cohesion = new Vector3(0,40,0); //transform.position;
            var cohesion = this.leaderPosition;

            // Looks up nearby boids.
            var neighborCount = 0;
            foreach (var neighbor in _boids)
            {
                if (neighbor == self) continue;

                var dist = Vector3.Distance(self.position, neighbor.position);
                if (dist > _neighborDistance) continue;

                // Influence from this boid
                separation += GetSeparationVector(self, neighbor);
                alignment += neighbor.rotation * this.path.GetDirection();
                cohesion += neighbor.position;

                neighborCount++;
            }

            // Normalization
            var div = 1.0f / (neighborCount + 1);
            alignment *= div;
            cohesion = (cohesion * div - self.position).normalized;

            // Calculate the target direction and convert to quaternion.
            var direction = separation + alignment * 0.667f + cohesion;
            var rotation = Quaternion.FromToRotation(this.path.GetDirection(), direction.normalized);

            // Applys the rotation with interpolation.
            if (rotation != self.rotation)
            {
                //deltaTime = フレーム間の秒数, 0.016秒あたり
                //x = 負の無限~0の場合，0 < e(x) < 1(x=0)
                //x = 正の無限の場合，e(x) > 1
                var ip = Mathf.Exp(-_rotationSpeed * Time.deltaTime);
                //if (this._boids[0] == self) { Debug.Log("ip: " + ip); }

                //_rotationSpeedがあがるとip=0.9くらいになり，self.rotationに近く
                //_rotationSpeedが少し下がるとip=0.3くらいになり，rotationに近くなる=targetに近づきすぎる
                self.rotation = Quaternion.Slerp(rotation, self.rotation, ip); //ipの分だけrotationからself.rotationへ近づく
            }
        }

        // Position updater
        void AdvanceBoid(Boid self)
        {
            //PerlinNoise（なだらかに変動するランダム）によって時間による速度のグラデーションを作り出す．
            //x = Time.time * 0.5f -> 1s=0.5, 2s=1.0, 3s=1.5, 4s=2.0
            //y = self.noiseOffset, each boid has different noiseOffset
            var noise = Mathf.PerlinNoise(Time.time * 0.5f, self.noiseOffset) * 2 - 1; //-1.0 ~ 1.0

            //noise * _velocityVarianceが分散値
            //_velocityVariance = 0.6, noise = -1.0 ~ 1.0 → (1 + noise * _velocityVariance) = -1.6 ~ 1.6
            //つまり，_velocity = 6だと, velocity=4.0-10.0くらい, 1が6を維持するためのもの
            var velocity = _velocity * (1 + noise * _velocityVariance); //4.0-9.9

            //velocity += this.Accelerate(self);

            var forward = self.rotation * this.path.GetDirection();
            //self.position += (forward * velocity + _scroll) * Time.deltaTime;
            //self.position += (forward * velocity) * Time.deltaTime; //スクロール０
            var deltaPosition = (forward * velocity) * Time.deltaTime;
            //Debug.Log("rotation: " + self.rotation.eulerAngles);
            //Debug.Log("deltaPostion: " + deltaPosition);
            self.position += deltaPosition;
        }
        #endregion

        #region Public methods

        GameObject _template;

        public void Spawn()
        {
            Spawn(transform.position + Random.insideUnitSphere * _spawnRadius);
        }

        public void Spawn(Vector3 position)
        {
            var go = Instantiate(_template);
            go.transform.parent = transform;
            go.SetActive(true);

            _boids.Add(new Boid() {
                position = position,
                rotation = Quaternion.Slerp(transform.rotation, Random.rotation, 0.3f),
                noiseOffset = Random.value * 10,
                gameObject = go
            });
        }

        #endregion

        #region MonoBehaviour implementation

        void Start()
        {
            this.path = new Path();
            this.animeRate = 0.0f;

            _template = transform.GetChild(0).gameObject; //パーティクルをとってくる
            _template.SetActive(false);

            for (var i = 0; i < _spawnCount; i++) Spawn();
        }

        void Update()
        {
            if (animeRate > 1.0f) {
                this.path.currentIndex = (this.path.currentIndex + 1) % path.GetCount();
                this.animeRate = 0.0f;
            }
            Debug.Log("rate: " + this.animeRate);
            this.leaderPosition = Vector3.Lerp(
                this.path.GetPosition(), this.path.GetNextPosition(), this.animeRate
            );
            Debug.Log("position: " + this.leaderPosition);
            this.leaderDirection = this.path.GetDirection();
            GameObject.Find("LeaderNode").transform.position = this.leaderPosition;

            foreach (var boid in _boids) SteerBoid(boid);
            foreach (var boid in _boids) AdvanceBoid(boid);
            //'foreach (var boid in _boids) PathFollowing(boid);

            foreach (var boid in _boids) {
                var tr = boid.gameObject.transform;
                tr.position = boid.position;
                tr.rotation = boid.rotation;
            }

            this.animeRate+=0.005f;
        }

        #endregion
    }
}
