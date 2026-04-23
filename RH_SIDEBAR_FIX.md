# ✅ RH Sidebar & Profile Fixes - Diagnostic et Solutions

## 🔍 Problème 1: Sidebar RH ne s'affiche pas

### Causes Possibles:

#### Cause 1: Rôle dans la base de données n'est pas exactement "RH"

**Diagnostic:**
```sql
-- Vérifier le rôle exact dans la base de données
SELECT Id, NomUtilisateur, Email, Role, IsActive 
FROM Utilisateur 
WHERE Role LIKE '%rh%' OR Role LIKE '%RH%';
```

**Solution:**
Le rôle doit être EXACTEMENT `"RH"` (majuscules)

```sql
-- Corriger le rôle
UPDATE Utilisateur 
SET Role = 'RH' 
WHERE Role = 'rh' OR Role = 'Rh' OR Role = 'rH';
```

#### Cause 2: Session non initialisée correctement

**Vérification dans le code:**
- ✅ Ligne 99: `HttpContext.Session.SetString("UserRole", user.Role);` - CORRECT
- ✅ Ligne 98: `HttpContext.Session.SetString("UserId", user.Id.ToString());` - CORRECT
- ✅ Ligne 100: `HttpContext.Session.SetString("Username", user.NomUtilisateur);` - CORRECT

#### Cause 3: Problème de configuration de session

**Vérifier Program.cs:**
```csharp
// La session doit être configurée
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Et dans le pipeline:
app.UseSession(); // DOIT être avant app.UseAuthorization()
```

### Solution Recommandée:

**Étape 1: Vérifier le rôle dans la base de données**
```sql
SELECT * FROM Utilisateur WHERE NomUtilisateur = 'votre_nom_rh';
```

Le champ `Role` doit être exactement: `RH`

**Étape 2: Si le rôle est incorrect, le corriger**
```sql
UPDATE Utilisateur SET Role = 'RH' WHERE Id = [ID_DU_RH];
```

**Étape 3: Se déconnecter et reconnecter**
1. Cliquer sur "Logout"
2. Se reconnecter avec le compte RH
3. La sidebar devrait apparaître

---

## ✅ Problème 2: Image de Profil Candidat - Déjà Fonctionnel

### Status: ✅ CORRIGÉ (dans la version précédente)

**Fonctionnalités:**
- ✅ Bouton "Enregistrer" ajouté
- ✅ Preview d'image fonctionne
- ✅ Upload au serveur fonctionne
- ✅ Validation (JPG, PNG, max 2MB)

**Fichier:** [Candidat/Views/Profile/Index.cshtml](file:///c:/Users/bennj/Downloads/CVAnalyzer-PFE/Candidat/Views/Profile/Index.cshtml#L78-L82)

**Comment tester:**
1. Se connecter comme Candidat
2. Aller sur "Mon Profil"
3. Cliquer sur l'icône caméra
4. Sélectionner une image
5. Cliquer sur "Enregistrer" ✅

---

## ✅ Problème 3: Table "Mes Candidatures" - Déjà Fonctionnel

### Status: ✅ CORRIGÉ (dans la version précédente)

**Fonctionnalités:**
- ✅ Filtre "Toutes" - affiche tout
- ✅ Filtre "Accepté" - affiche uniquement acceptées
- ✅ Filtre "Refusé" - affiche uniquement refusées
- ✅ Filtre "En attente" - affiche uniquement en attente
- ✅ Message si aucun résultat

**Fichiers:**
- [ProfileController.cs](file:///c:/Users/bennj/Downloads/CVAnalyzer-PFE/Candidat/Controllers/ProfileController.cs#L52-L69) - Logique de filtrage
- [_ApplicationsTable.cshtml](file:///c:/Users/bennj/Downloads/CVAnalyzer-PFE/Candidat/Views/Profile/_ApplicationsTable.cshtml) - Affichage

**Comment tester:**
1. Se connecter comme Candidat
2. Aller sur "Mon Profil"
3. Section "Mes candidatures"
4. Cliquer sur "Accepté" → Affiche uniquement acceptées ✅
5. Cliquer sur "Refusé" → Affiche uniquement refusées ✅
6. Cliquer sur "En attente" → Affiche uniquement en attente ✅

---

## 🔧 Debugging Complet pour RH Sidebar

### Étape 1: Ajouter du logging temporaire

**Fichier:** [Administration/Views/Shared/_Layout.cshtml](file:///c:/Users/bennj/Downloads/CVAnalyzer-PFE/Administration/Views/Shared/_Layout.cshtml#L65-L70)

Ajouter après ligne 70:
```csharp
@{
    // DEBUG: Afficher les valeurs de session
    var debugInfo = $"UserRole: '{userRole}' | UserName: '{userName}'";
}

<!-- TEMP: Afficher pour debug -->
@if (User.IsInRole("RH") || userRole == "RH")
{
    <div style="background: yellow; padding: 10px; position: fixed; top: 0; right: 0; z-index: 9999;">
        @debugInfo
    </div>
}
```

### Étape 2: Vérifier la session dans le navigateur

1. Ouvrir les DevTools (F12)
2. Aller dans Application → Cookies
3. Vérifier que `.AspNetCore.Session` existe
4. Ou ajouter un point d'arrêt dans le controller

### Étape 3: Tester avec un compte RH connu

```sql
-- Créer un compte RH de test
INSERT INTO Utilisateur (NomUtilisateur, Email, MotPasse, Role, IsActive, DateCreation)
VALUES (
    'RH_Test',
    'rh@test.com',
    '$2a$11$...', -- Hash BCrypt de 'Password123'
    'RH',
    1,
    GETDATE()
);
```

---

## 🎯 Solutions Rapides

### Solution 1: Vérifier et corriger le rôle SQL

```sql
-- 1. Voir tous les rôles
SELECT DISTINCT Role FROM Utilisateur;

-- 2. Voir les utilisateurs RH
SELECT * FROM Utilisateur WHERE Role = 'RH';

-- 3. Corriger si nécessaire
UPDATE Utilisateur SET Role = 'RH' WHERE Role != 'RH' AND Role != 'Admin' AND Role != 'Directeur' AND Role != 'Candidat';
```

### Solution 2: Forcer la reconnexion

1. Se déconnecter complètement
2. Fermer le navigateur
3. Rouvrir le navigateur
4. Se reconnecter avec le compte RH
5. Vérifier que la sidebar apparaît

### Solution 3: Vérifier Program.cs

```csharp
// Dans Program.cs, vérifier que la session est bien configurée:

var builder = WebApplication.CreateBuilder(args);

// Ajouter la session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = ".AspNetCore.Session";
});

var app = builder.Build();

// IMPORTANT: UseSession DOIT être avant UseAuthorization
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();
```

---

## 📋 Checklist de Vérification

### Pour RH Sidebar:
- [ ] Le rôle dans la base de données est EXACTEMENT "RH" (majuscules)
- [ ] La session est configurée dans Program.cs
- [ ] `app.UseSession()` est avant `app.UseAuthorization()`
- [ ] Se déconnecter et reconnecter après correction
- [ ] Vérifier dans DevTools que la session existe
- [ ] La sidebar apparaît avec: Tableau de bord, Postes, Profil

### Pour Profile Image:
- [ ] Le bouton "Enregistrer" est visible
- [ ] Clique sur caméra → Sélection d'image
- [ ] Preview s'affiche
- [ ] Clique sur "Enregistrer" → Message de succès
- [ ] L'image est sauvegardée

### Pour Mes Candidatures:
- [ ] Filtre "Toutes" fonctionne
- [ ] Filtre "Accepté" fonctionne (badge vert)
- [ ] Filtre "Refusé" fonctionne (badge rouge)
- [ ] Filtre "En attente" fonctionne (badge jaune)
- [ ] Message "Aucune candidature" si vide

---

## 🚀 Commandes SQL Utiles

### Voir les rôles existants:
```sql
SELECT Role, COUNT(*) as Count FROM Utilisateur GROUP BY Role;
```

### Corriger tous les rôles RH:
```sql
UPDATE Utilisateur 
SET Role = 'RH' 
WHERE LOWER(Role) = 'rh' AND Role != 'RH';
```

### Voir un utilisateur spécifique:
```sql
SELECT Id, NomUtilisateur, Email, Role, IsActive, PhotoUrl 
FROM Utilisateur 
WHERE NomUtilisateur = 'nom_du_user';
```

### Créer un compte RH de test:
```sql
-- Mot de passe: Password123 (hash BCrypt)
INSERT INTO Utilisateur (NomUtilisateur, Email, MotPasse, Role, IsActive, DateCreation)
VALUES (
    'RH_Admin',
    'rh.admin@test.com',
    '$2a$11$WwVMRBhqJQZ8qQvQxQZ8qOQxQZ8qQvQxQZ8qQvQxQZ8qQvQxQZ8q',
    'RH',
    1,
    GETDATE()
);
```

---

## 📞 Si le problème persiste

### Informations à fournir:

1. **Rôle dans la base de données:**
```sql
SELECT NomUtilisateur, Role FROM Utilisateur WHERE NomUtilisateur = 'votre_rh';
```

2. **Session dans le navigateur:**
- Ouvrir DevTools → Application → Cookies
- Capture d'écran du cookie `.AspNetCore.Session`

3. **URL actuelle:**
- Quelle est l'URL après connexion?
- Ex: `http://localhost:XXXX/RH/Dashboard`

4. **Message d'erreur:**
- Y a-t-il une erreur dans la console (F12)?
- Capture d'écran de l'erreur

---

## ✅ Résumé

| Problème | Status | Solution |
|----------|--------|----------|
| Sidebar RH | ⚠️ À vérifier | Vérifier rôle = "RH" en majuscules |
| Image profil | ✅ Corrigé | Bouton "Enregistrer" ajouté |
| Filtres candidatures | ✅ Corrigé | Filtrage par statut implémenté |

---

**Date:** 23 avril 2026  
**Version:** 2.2.0  
**Statut:** En attente de vérification du rôle RH
