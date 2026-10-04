# GameCerdas_Praktikum05_EnemyFSM

## Identitas

| Keterangan | Isi |
|---|---|
| Nama Kelompok | Xx_Sigm4_M4le_67_xX |
| Anggota 1 | Muhammad Farrel Fathin Wibowo / 5025231233 |
| Anggota 2 | Danny Rachmadian Yusuf Satryatama / 5025231240 |
| Anggota 3 | Valensio Arvin Putra Setiawan / 5025231273 |

## Praktikum: Enemy AI dengan Finite State Machine (FSM)

### Tujuan Praktikum

Pada praktikum ini mahasiswa akan membuat sebuah NPC enemy yang mampu mengambil keputusan menggunakan Finite State Machine (FSM).

Enemy memiliki empat perilaku utama:

```plaintext
Patrol
   ↓
Chase
   ↓
Attack
   ↓
Flee
```

serta satu state tambahan:

```plaintext
Dead
```

Perilaku yang dibuat:

1. Enemy melakukan patroli di antara beberapa waypoint.
2. Enemy mendeteksi player menggunakan jarak, field of view, dan line of sight.
3. Jika player terlihat, enemy berpindah dari Patrol ke Chase.
4. Enemy mengejar player menggunakan `NavMeshAgent`.
5. Jika player berada dalam jarak serang, enemy berpindah ke Attack.
6. Enemy menyerang menggunakan damage dan attack cooldown.
7. Jika health enemy turun melewati batas tertentu, enemy masuk ke Flee.
8. Enemy menuju safe point ketika kabur.
9. Jika enemy sudah cukup aman, enemy kembali melakukan patrol.
10. Jika health mencapai 0, enemy masuk ke Dead.

### Fitur

Seluruh praktikum berada pada satu scene: `Assets/Scenes/Praktikum05_FSM.unity`.

**Inti praktikum**

- `EnemyFSM` dengan lima state: Patrol, Chase, Attack, Flee, dan Dead.
- Persepsi enemy (`EnemyPerception`) berdasarkan jarak, sudut pandang, dan raycast line of sight.
- Pergerakan enemy memakai `NavMeshAgent` pada NavMesh yang di-bake dari scene.
- Sistem health untuk enemy dan player, serta serangan enemy dengan damage dan cooldown.
- Player digerakkan dengan keyboard dan dapat menyerang enemy dengan tombol Space.

**Bonus challenge yang dikerjakan**

- Tampilan UI untuk current state enemy.
- Animasi untuk enemy.

**Improvisasi dari tim**

- Slider health point untuk enemy.
- HUD sederhana untuk player yang menampilkan total HP beserta slider-nya.
- Animasi juga dikembangkan untuk player (Idle, Run, Attack, dan Death), bersamaan dengan penalty ketika player mati: player tidak bisa bergerak maupun menyerang, dan enemy berhenti menganggapnya sebagai target.

> Keterangan di atas hanya preview singkat pencapaian praktikum. Penjelasan lengkap ada di bagian berikutnya.

---

## Setup Scene

- **Ground**, **Environment**, dan **Obstacles** (`Wall1`–`Wall12`) membentuk area permainan. Semuanya ikut di-bake pada **Navigation**.
- **Navigation** menyimpan `NavMeshSurface` yang menghasilkan area yang bisa dilalui oleh `NavMeshAgent`.
- **PatrolPoints** berisi waypoint (`W1`–`W4`) yang dilalui enemy saat patrol, dan **SafePoint** adalah titik tujuan enemy saat kabur.
- **Enemy** memakai komponen `NavMeshAgent`, `EnemyHealth`, `EnemyPerception`, `EnemyFSM`, dan `EnemyAnimation`, dengan model beranimasi `TripleT+Rigs`.
- **Player** memakai `CharacterController`, `PlayerHealth`, `SimplePlayerController`, `PlayerAttackTest`, dan `PlayerAnimation`, dengan model beranimasi `Wok`.
- **EnemyUI** adalah Canvas *World Space* di atas kepala enemy yang berisi **Text_Enemy_State** dan **Slider_Enemy_Health**.
- **PlayerHUD** adalah Canvas *Screen Space* yang berisi **Slider_HP_Player** dan **Text_HP**.

Pengaturan yang dipakai:

| Komponen | Parameter | Nilai |
|---|---|---|
| `EnemyPerception` | `visionRange` / `visionAngle` / `eyeHeight` | 10 / 80° / 1 |
| `EnemyFSM` (Patrol) | `patrolSpeed` / `waypointTolerance` | 2 / 0.5 |
| `EnemyFSM` (Chase) | `chaseSpeed` / `lostPlayerDelay` | 7 / 2 detik |
| `EnemyFSM` (Attack) | `attackRange` / `attackExitRange` | 2 / 2.75 |
| `EnemyFSM` (Attack) | `attackDamage` / `attackCooldown` | 10 / 1.5 detik |
| `EnemyFSM` (Flee) | `fleeSpeed` / `lowHealthThreshold` / `safeDistance` | 5 / 30 / 10 |
| `EnemyHealth`, `PlayerHealth` | `maxHealth` | 100 |
| `SimplePlayerController` | `moveSpeed` | 8 |
| `PlayerAttackTest` | `attackDistance` / `damage` | 3 / 20 |

### Diagram State

```plaintext
                 player terlihat
      Patrol ───────────────────────▶ Chase
        ▲  ◀─────────────────────────   │
        │   player hilang ≥ 2 detik     │ jarak ≤ attackRange
        │                               ▼
        │   tidak terlihat atau       Attack
        │   jarak > attackExitRange     │ (kembali ke Chase)
        │
        │  health ≤ 30 (sekali)  ┌──────────────┐
        └────────────────────────│     Flee     │
          aman / tiba di SafePoint└──────────────┘

  Dari state mana pun: health = 0 ───────────────▶ Dead
```

### Arti Pengaturan Animasi

**Enemy** (`TripleT+Rigs.controller`) memakai parameter `State` (Int) yang nilainya mengikuti urutan enum `EnemyState`:

| Nilai `State` | State FSM | Clip |
|---|---|---|
| 0 | Patrol | `EnemyWalking` |
| 1 | Chase | `Fast Run` |
| 2 | Attack | `Melee Attack` |
| 3 | Flee | `Fast Run` |
| 4 | Dead | `Death` |

Semua transisi berasal dari **Any State** dengan satu kondisi `State == n`. *Has Exit Time* dimatikan, dan *Can Transition To Self* juga dimatikan agar animasi tidak diulang dari awal selama state yang sama masih aktif.

**Player** (`Wok.controller`) memakai tiga parameter:

| Parameter | Tipe | Fungsi |
|---|---|---|
| `Speed` | Float | `idleAnim` ↔ `FastRun` (ambang 0,1) |
| `Attack` | Trigger | Any State → `punchCombo`, lalu kembali ke `idleAnim` setelah clip selesai |
| `IsDead` | Bool | Any State → `Death` |

---

## Penjelasan Logika Kode

Penjelasan mengikuti urutan: `EnemyHealth`, `EnemyPerception`, `EnemyFSM`, `EnemyAnimation`, `EnemyUI`, lalu script player: `PlayerHealth`, `SimplePlayerController`, `PlayerAttackTest`, `PlayerAnimation`, dan `PlayerHUD`.

#### 1. `Assets/Scripts/EnemyHealth.cs`

**Field.** `maxHealth` dan properti `CurrentHealth` (hanya bisa diubah dari dalam script ini). `MaxHealth` dan `IsDead` (health ≤ 0) dibaca oleh script lain.

**`Awake()`**
Health diisi penuh saat game mulai.

**`TakeDamage(damage)`**
Jika enemy sudah mati, fungsi berhenti. Jika belum, health dikurangi `damage` lalu dibatasi antara 0 dan `maxHealth`, dan nilainya ditulis ke Console.

#### 2. `Assets/Scripts/EnemyPerception.cs`

Script ini hanya menjawab pertanyaan "apakah enemy melihat player sekarang?", dan hasilnya dibaca `EnemyFSM`.

**Field.** Target `player`, `visionRange`, `visionAngle`, `eyeHeight` (tinggi mata enemy), dan `obstacleMask` (layer yang dianggap menghalangi pandangan). Hasilnya disimpan di `CanSeePlayer`. `DistanceToPlayer` menghitung jarak ke player.

**`Awake()`**
Mengambil `PlayerHealth` dari player untuk memeriksa apakah player sudah mati.

**`Update()`**
Memperbarui `CanSeePlayer` setiap frame lewat `CheckPlayerVisibility()`.

**`CheckPlayerVisibility()`**
Mengembalikan `true` hanya jika keempat pemeriksaan lolos:
1. **Player masih hidup.** Jika player mati, enemy langsung tidak lagi melihatnya.
2. **Jarak.** Jarak dari mata enemy ke dada player tidak boleh melebihi `visionRange`.
3. **Sudut pandang.** Sudut antara arah depan enemy dan arah ke player tidak boleh melebihi setengah `visionAngle`.
4. **Line of sight.** Ray ditembakkan dari mata enemy ke player sejauh jarak tersebut. Jika ray mengenai objek pada `obstacleMask`, pandangan terhalang.

**`OnDrawGizmosSelected()`**
Menggambar lingkaran jangkauan dan dua garis batas sudut pandang di Scene view, supaya area penglihatan mudah dicek.

#### 3. `Assets/Scripts/EnemyFSM.cs`

**`EnemyState`**
Enum berisi lima state: `Patrol`, `Chase`, `Attack`, `Flee`, dan `Dead`.

**Field.** Referensi (player, `NavMeshAgent`, `EnemyPerception`, `EnemyHealth`), parameter setiap state (lihat tabel pengaturan), `patrolPoints`, `safePoint`, serta `currentState` yang terlihat di Inspector untuk debugging. Properti publik `CurrentState` dipakai oleh `EnemyAnimation` dan `EnemyUI`. Variabel internal: `currentPatrolIndex`, `lostPlayerTimer`, `nextAttackTime`, dan `fleeTriggered`.

**`Awake()`**
Mengisi referensi yang kosong dengan `GetComponent`, dan mengambil `PlayerHealth` dari player.

**`Start()`**
Memulai FSM pada state `Patrol`.

**`Update()`**
Dalam satu frame, urutannya:
1. **Transisi global (prioritas tertinggi).** Jika enemy mati, state langsung menjadi `Dead` dan fungsi berhenti. Jika health ≤ `lowHealthThreshold` dan Flee belum pernah dipicu, state menjadi `Flee`. Pemeriksaan ini dilakukan dari state mana pun.
2. **Update state aktif.** Lewat `switch`, dijalankan fungsi `UpdatePatrol`, `UpdateChase`, `UpdateAttack`, `UpdateFlee`, atau `UpdateDead` sesuai `currentState`.

**`ChangeState(newState)`**
Satu-satunya tempat state diganti. Jika state tujuan sama, tidak terjadi apa pun. Selain itu urutannya: `ExitState` untuk state lama, `currentState` diganti, nama state ditulis ke Console, lalu `EnterState` untuk state baru.

**`EnterState(state)`**
Menyiapkan state yang baru dimasuki:
- **Patrol:** agent berjalan dengan `patrolSpeed` menuju waypoint saat ini.
- **Chase:** agent berjalan dengan `chaseSpeed`, dan timer kehilangan player di-reset.
- **Attack:** agent dihentikan dan path-nya dibersihkan.
- **Flee:** agent berjalan dengan `fleeSpeed` menuju `safePoint`.
- **Dead:** agent dihentikan dan path-nya dibersihkan.

**`ExitState(state)`**
Saat keluar dari Attack, agent dijalankan kembali (`isStopped = false`).

**`UpdatePatrol()`**
1. Jika player terlihat, pindah ke `Chase`.
2. Jika belum, dan agent sudah dekat dengan waypoint (jarak ≤ `waypointTolerance`, path tidak sedang dihitung), indeks waypoint dinaikkan (kembali ke 0 setelah yang terakhir) dan tujuan agent diperbarui lewat `SetPatrolDestination()`.

**`UpdateChase()`**
- **Player terlihat:** timer di-reset, tujuan agent diperbarui ke posisi player, dan jika jarak ≤ `attackRange`, pindah ke `Attack`.
- **Player tidak terlihat:** timer bertambah, dan jika sudah mencapai `lostPlayerDelay`, enemy menyerah dan kembali ke `Patrol`.

**`UpdateAttack()`**
1. Enemy menoleh ke player lewat `FacePlayer()`.
2. Jika player tidak terlihat atau jarak > `attackExitRange`, pindah kembali ke `Chase`.
3. Jika tidak, dan waktu sudah melewati `nextAttackTime`, enemy menyerang lewat `AttackPlayer()`, lalu `nextAttackTime` dijadwalkan `attackCooldown` detik ke depan.

**`AttackPlayer()`**
Memberi `attackDamage` ke `PlayerHealth`.

**`FacePlayer()`**
Memutar enemy perlahan (`Slerp`) menghadap player pada bidang datar (sumbu Y diabaikan).

**`UpdateFlee()`**
Jika jarak ke player ≥ `safeDistance`, atau enemy sudah tiba di dekat `safePoint` (jarak ≤ 1), pindah kembali ke `Patrol`. Karena `fleeTriggered` sudah bernilai `true`, Flee hanya dipicu satu kali sehingga enemy tidak terus kabur setiap kali health rendah.

**`UpdateDead()`**
Kosong. Enemy tidak melakukan aksi apa pun.

**`OnDrawGizmosSelected()`**
Menggambar `attackRange` (merah) dan `attackExitRange` (magenta) di Scene view.

#### 4. `Assets/Scripts/EnemyAnimation.cs`

Menghubungkan FSM dengan Animator tanpa mengubah `EnemyFSM`.

**`Awake()`**
Mencari `EnemyFSM` dan `Animator` jika belum diisi.

**`Update()`**
Hanya saat state berubah, nilai enum diubah menjadi angka dan dikirim ke parameter Int `State` di Animator (`(int)lastState`: Patrol 0, Chase 1, Attack 2, Flee 3, Dead 4). Transisi di Animator Controller kemudian memilih clip yang diputar.

#### 5. `Assets/Scripts/EnemyUI.cs`

Dipasang di Canvas World Space di atas enemy.

**`Awake()`**
Menyimpan kamera utama dan mencari `EnemyFSM` serta `EnemyHealth` dari parent bila belum diisi.

**`LateUpdate()`**
Setiap frame:
1. Canvas dibuat menghadap searah kamera (billboard), sehingga teks selalu terbaca.
2. Teks diisi nama state dan diwarnai lewat `StateColor()`: hijau (Patrol), kuning (Chase), merah (Attack), cyan (Flee), abu-abu (Dead).
3. Slider health diisi dengan `CurrentHealth / MaxHealth`.

#### 6. `Assets/Scripts/PlayerHealth.cs`

**Field.** `maxHealth` dan properti `CurrentHealth`, `MaxHealth`, dan `IsDead`. Event `Died` dipanggil satu kali saat health mencapai 0.

**`TakeDamage(damage)`**
Jika player sudah mati, damage diabaikan. Jika belum, health dikurangi dan dibatasi antara 0 dan `maxHealth`. Saat health mencapai 0, pesan `Player Dead` ditulis dan event `Died` dipanggil.

#### 7. `Assets/Scripts/SimplePlayerController.cs`

**`Update()`**
1. Jika player mati, fungsi berhenti sehingga player tidak bisa bergerak.
2. Input WASD atau panah dibaca, dinormalisasi, lalu player digerakkan lewat `CharacterController.Move` dengan kecepatan `moveSpeed`.
3. Gravitasi diterapkan dengan `Move` terpisah, supaya player tetap menempel di lantai.
4. Jika player bergerak, arah hadapnya mengikuti arah gerak.

#### 8. `Assets/Scripts/PlayerAttackTest.cs`

**`Update()`**
Jika player mati, tidak ada yang terjadi. Saat tombol Space ditekan, trigger `Attack` dikirim ke Animator, lalu `TryAttackEnemy()` dipanggil.

**`TryAttackEnemy()`**
Jika jarak ke enemy ≤ `attackDistance`, enemy diberi `damage`. Animasi bersifat hiasan, sehingga damage terjadi seketika saat Space ditekan.

#### 9. `Assets/Scripts/PlayerAnimation.cs`

**`LateUpdate()`**
Kecepatan horizontal dihitung dari perubahan posisi per detik, bukan dari `CharacterController.velocity`. Alasannya, `velocity` hanya mencerminkan `Move()` terakhir, yaitu gerak vertikal (gravitasi), sehingga nilainya selalu mendekati 0. Nilai kecepatan dikirim ke parameter `Speed`, dan `health.IsDead` dikirim ke parameter `IsDead`.

#### 10. `Assets/Scripts/PlayerHUD.cs`

**`Update()`**
Slider diisi dengan `CurrentHealth / MaxHealth`, dan teks menampilkan `HP sekarang/maksimum`.

---

## Pertanyaan Analisis

### 1. Apa perbedaan State, Condition, dan Transition?

**State** adalah kondisi atau perilaku enemy saat ini (Patrol, Chase, Attack, Flee, Dead). **Condition** adalah syarat yang diperiksa, misalnya `CanSeePlayer` atau `health <= 30`. **Transition** adalah perpindahan dari satu state ke state lain yang terjadi ketika condition terpenuhi, seperti Patrol → Chase ketika player terlihat.

### 2. Mengapa NavMeshAgent bukan merupakan FSM?

`NavMeshAgent` hanya mengatur cara bergerak ke tujuan: mencari path dan menggerakkan NPC. Ia tidak punya state perilaku maupun aturan perpindahan, serta tidak memutuskan kapan harus patroli, mengejar, menyerang, atau kabur. Keputusan itu dibuat oleh FSM, sedangkan `NavMeshAgent` hanya dipakai sebagai alat gerak.

### 3. Mengapa Line of Sight menggunakan Raycast?

Jarak dan sudut pandang saja tidak tahu apakah ada dinding di antara enemy dan player. Raycast menembakkan garis lurus dari mata enemy ke player dan memeriksa apakah ada obstacle yang terkena lebih dulu. Jika ada, pandangan terhalang. Tanpa raycast, enemy bisa "melihat" menembus dinding.

### 4. Mengapa Attack membutuhkan cooldown?

Tanpa cooldown, serangan dijalankan setiap frame sehingga player akan kehilangan seluruh health dalam sekejap. Cooldown membatasi kecepatan serangan (1,5 detik antar serangan) sehingga pertarungan seimbang dan player punya waktu bereaksi.

### 5. Mengapa Attack Range dan Attack Exit Range dibuat berbeda?

Agar tidak terjadi flickering. Jika batas masuk dan keluar sama (2), player yang berada tepat di perbatasan membuat enemy bolak-balik Attack ↔ Chase setiap frame. Dengan `attackExitRange` (2,75) lebih besar dari `attackRange` (2), enemy baru kembali ke Chase setelah player benar-benar menjauh, sehingga perpindahan state lebih stabil (hysteresis).

### 6. Mengapa transition menuju Dead harus memiliki prioritas tinggi?

Karena enemy yang sudah mati tidak boleh melakukan apa pun lagi. Jika transition Dead diperiksa setelah yang lain, enemy bisa tetap mengejar, menyerang, atau berpindah ke Flee padahal health-nya 0. Karena itu pemeriksaan Dead berada paling atas di `Update()` dan langsung `return`, sehingga tidak ada logika state lain yang berjalan.
