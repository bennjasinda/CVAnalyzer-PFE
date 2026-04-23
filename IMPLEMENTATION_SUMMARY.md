# Système de Notifications et Profil Candidat - Implémentation

## 📋 Résumé des Fonctionnalités Implémentées

Ce document décrit les fonctionnalités ajoutées au système CVAnalyzer-PFE pour le système de notifications et la gestion des profils candidats.

---

## 📩 1. SYSTÈME DE NOTIFICATIONS

### 1.1 Notifications pour les Candidats

**Déclencheurs:**
- ✅ CV accepté → Notification envoyée au candidat
- ❌ CV refusé → Notification envoyée au candidat

**Messages:**
- **Acceptation:** "Votre candidature a été acceptée. Veuillez attendre, vous serez contacté via votre email ou numéro de téléphone."
- **Refus:** "Votre candidature a été refusée. Nous vous remercions pour votre intérêt."

**Implémentation:**
- [RHController.cs](file:///c:/Users/bennj/Downloads/CVAnalyzer-PFE/Administration/Controllers/RHController.cs) - Lignes 368-431
- [DirecteurDepartementController.cs](file:///c:/Users/bennj/Downloads/CVAnalyzer-PFE/Administration/Controllers/DirecteurDepartementController.cs) - Lignes 330-393

### 1.2 Notifications pour les RH

**Déclencheurs:**
- 📄 Nouveau CV reçu pour un poste
- ✅ CV accepté par un directeur

**Messages:**
- **Nouveau CV:** "Un nouveau CV a été déposé pour le poste '{Titre du poste}'."
- **CV Accepté:** "Un directeur a accepté une candidature pour le poste '{Titre du poste}'."

**Implémentation:**
- [OffreController.cs](file:///c:/Users/bennj/Downloads/CVAnalyzer-PFE/Candidat/Controllers/OffreController.cs) - Lignes 246-284
- [DirecteurDepartementController.cs](file:///c:/Users/bennj/Downloads/CVAnalyzer-PFE/Administration/Controllers/DirecteurDepartementController.cs) - Lignes 349-362

### 1.3 Notifications pour les Directeurs

**Déclencheurs:**
- 📄 Nouveau CV lié à leur département

**Message:**
- "Un nouveau CV a été déposé pour le poste '{Titre du poste}' dans votre département."

**Implémentation:**
- [OffreController.cs](file:///c:/Users/bennj/Downloads/CVAnalyzer-PFE/Candidat/Controllers/OffreController.cs) - Lignes 261-282

### 1.4 Affichage des Notifications

**Pour les Candidats:**
- Onglet "Notifications" dans le profil candidat
- Affichage des 20 dernières notifications
- Indicateur visuel pour les notifications non lues
- Badges de couleur selon le type (Succès, Danger, Info)

**Fichier:** [Index.cshtml](file:///c:/Users/bennj/Downloads/CVAnalyzer-PFE/Candidat/Views/Profile/Index.cshtml) - Lignes 126-177

---

## 👤 2. PROFIL CANDIDAT AMÉLIORÉ

### 2.1 Nouveaux Champs de Profil

Le modèle `Utilisateur` a été étendu avec les champs suivants:

| Champ | Type | Description |
|-------|------|-------------|
| `Telephone` | string | Numéro de téléphone du candidat |
| `Competences` | string (MAX) | Liste des compétences (séparées par virgules) |
| `Experiences` | string (MAX) | Expériences professionnelles détaillées |
| `Diplomes` | string (MAX) | Diplômes et formations |
| `GitLink` | string (500) | URL du profil GitHub |
| `LinkedInLink` | string (500) | URL du profil LinkedIn |
| `Bio` | string (MAX) | Brève description du profil |

**Fichiers modifiés:**
- [Candidat/Models/Utilisateur.cs](file:///c:/Users/bennj/Downloads/CVAnalyzer-PFE/Candidat/Models/Utilisateur.cs)
- [Administration/Models/Utilisateur.cs](file:///c:/Users/bennj/Downloads/CVAnalyzer-PFE/Administration/Models/Utilisateur.cs)
- [Candidat/Models/ViewModels/ProfilePageViewModel.cs](file:///c:/Users/bennj/Downloads/CVAnalyzer-PFE/Candidat/Models/ViewModels/ProfilePageViewModel.cs)

### 2.2 Contrôleur de Profil Mis à Jour

**Fichier:** [ProfileController.cs](file:///c:/Users/bennj/Downloads/CVAnalyzer-PFE/Candidat/Controllers/ProfileController.cs)

**Modifications:**
- Action `UpdateProfile` accepte maintenant les nouveaux champs
- Sauvegarde automatique des informations dans la base de données
- Validation et nettoyage des données (trim)

### 2.3 Interface Utilisateur du Profil

**Fichier:** [Index.cshtml](file:///c:/Users/bennj/Downloads/CVAnalyzer-PFE/Candidat/Views/Profile/Index.cshtml)

**Sections ajoutées:**

1. **Informations de base:**
   - Nom complet
   - Email
   - Téléphone
   - Bio/Description

2. **Compétences et Expérience:**
   - Compétences (textarea)
   - Expériences professionnelles (textarea)
   - Diplômes et Formation (textarea)

3. **Liens Sociaux:**
   - GitHub (avec icône)
   - LinkedIn (avec icône)

---

## 🔘 3. BOUTONS D'ACTION (RH & DIRECTEUR)

### 3.1 Boutons Disponibles

Les boutons suivants sont déjà implémentés dans les vues RH et Directeur:

| Bouton | Icône | Action |
|--------|-------|--------|
| ✔️ Voir CV | 👁️ | Affiche l'analyse détaillée du CV |
| ✅ Accepter | ✓ | Accepte la candidature et envoie une notification |
| ❌ Refuser | ✕ | Refuse la candidature et envoie une notification |

**Fichiers:**
- [RH/DetailPoste.cshtml](file:///c:/Users/bennj/Downloads/CVAnalyzer-PFE/Administration/Views/RH/DetailPoste.cshtml) - Lignes 912-932
- [DirecteurDepartement/DetailsPoste.cshtml](file:///c:/Users/bennj/Downloads/CVAnalyzer-PFE/Administration/Views/DirecteurDepartement/DetailsPoste.cshtml) - Lignes 886-910

### 3.2 Logique d'Affichage

Les boutons Accepter/Refuser ne sont affichés que si le statut du CV est "En attente".

```csharp
@if (cv.Statut == "En attente")
{
    // Boutons Accepter et Refuser
}
```

---

## 🗄️ 4. MIGRATION DE BASE DE DONNÉES

### 4.1 Script SQL

Un script SQL est fourni pour ajouter les nouveaux champs à la table `Utilisateur`:

**Fichier:** [AddCandidateProfileFields.sql](file:///c:/Users/bennj/Downloads/CVAnalyzer-PFE/Administration/Scripts/AddCandidateProfileFields.sql)

**Instructions:**
1. Ouvrir le script dans SQL Server Management Studio
2. Remplacer `[YourDatabaseName]` par le nom de votre base de données
3. Exécuter le script

### 4.2 Alternative: Entity Framework Migration

Quand l'application n'est pas en cours d'exécution:

```bash
cd Administration
dotnet ef migrations add AddCandidateProfileFields --context ApplicationDbContext
dotnet ef database update
```

---

## 🔄 5. FLUX DE NOTIFICATIONS

### 5.1 Candidat soumet un CV

```
Candidat → Dépose CV
    ↓
Notification créée pour:
    - Tous les RH ("Nouveau CV reçu")
    - Directeurs du département ("Nouveau CV dans votre département")
```

### 5.2 RH/Directeur accepte un CV

```
RH/Directeur → Clique "Accepter"
    ↓
Statut CV → "Accepte"
    ↓
Notifications créées pour:
    - Candidat ("Candidature acceptée")
    - Tous les RH ("Candidature acceptée par un directeur") [si action par directeur]
```

### 5.3 RH/Directeur refuse un CV

```
RH/Directeur → Clique "Refuser"
    ↓
Statut CV → "Refuse"
    ↓
Notification créée pour:
    - Candidat ("Candidature refusée")
```

---

## 📊 6. STRUCTURE DES DONNÉES

### 6.1 Table Notification

| Colonne | Type | Description |
|---------|------|-------------|
| `Id` | int | Clé primaire |
| `UtilisateurId` | int | ID du destinataire |
| `Titre` | string | Titre de la notification |
| `Message` | string | Message détaillé |
| `Type` | string | "Success", "Danger", ou "Info" |
| `IsRead` | bool | État de lecture |
| `DateCreation` | DateTime | Date de création |
| `RelatedCvId` | int? | ID du CV lié (optionnel) |
| `RelatedOffreId` | int? | ID de l'offre liée (optionnel) |

### 6.2 Table Utilisateur (Champs ajoutés)

| Colonne | Type | Nullable |
|---------|------|----------|
| `Telephone` | NVARCHAR(50) | Oui |
| `Competences` | NVARCHAR(MAX) | Oui |
| `Experiences` | NVARCHAR(MAX) | Oui |
| `Diplomes` | NVARCHAR(MAX) | Oui |
| `GitLink` | NVARCHAR(500) | Oui |
| `LinkedInLink` | NVARCHAR(500) | Oui |
| `Bio` | NVARCHAR(MAX) | Oui |

---

## ✅ 7. VÉRIFICATION ET TESTS

### 7.1 Tests à Effectuer

1. **Profil Candidat:**
   - [ ] Créer/modifier le profil avec tous les nouveaux champs
   - [ ] Vérifier la sauvegarde en base de données
   - [ ] Vérifier l'affichage des données

2. **Notifications Candidat:**
   - [ ] Déposer un CV → Vérifier notification RH/Directeur
   - [ ] RH accepte CV → Vérifier notification candidat
   - [ ] RH refuse CV → Vérifier notification candidat
   - [ ] Afficher l'onglet Notifications dans le profil

3. **Notifications RH:**
   - [ ] Nouveau CV déposé → Vérifier notification
   - [ ] Directeur accepte CV → Vérifier notification

4. **Notifications Directeur:**
   - [ ] Nouveau CV dans département → Vérifier notification

5. **Boutons d'action:**
   - [ ] Voir CV → Redirige vers l'analyse
   - [ ] Accepter CV → Change statut + notifications
   - [ ] Refuser CV → Change statut + notifications

### 7.2 Points de Vérification dans le Code

**Notifications:**
- [RHController.cs:377-400](file:///c:/Users/bennj/Downloads/CVAnalyzer-PFE/Administration/Controllers/RHController.cs#L377-L400) - Acceptation RH
- [RHController.cs:418-427](file:///c:/Users/bennj/Downloads/CVAnalyzer-PFE/Administration/Controllers/RHController.cs#L418-L427) - Refus RH
- [DirecteurDepartementController.cs:339-362](file:///c:/Users/bennj/Downloads/CVAnalyzer-PFE/Administration/Controllers/DirecteurDepartementController.cs#L339-L362) - Acceptation Directeur
- [DirecteurDepartementController.cs:380-389](file:///c:/Users/bennj/Downloads/CVAnalyzer-PFE/Administration/Controllers/DirecteurDepartementController.cs#L380-L389) - Refus Directeur
- [OffreController.cs:246-284](file:///c:/Users/bennj/Downloads/CVAnalyzer-PFE/Candidat/Controllers/OffreController.cs#L246-L284) - Nouveau CV

**Profil:**
- [ProfileController.cs:159-238](file:///c:/Users/bennj/Downloads/CVAnalyzer-PFE/Candidat/Controllers/ProfileController.cs#L159-L238) - UpdateProfile
- [Index.cshtml:65-117](file:///c:/Users/bennj/Downloads/CVAnalyzer-PFE/Candidat/Views/Profile/Index.cshtml#L65-L117) - Formulaire profil

---

## 🎯 8. AMÉLIORATIONS FUTURES SUGGÉRÉES

1. **Notifications en temps réel:** Utiliser SignalR pour les notifications push
2. **Notifications par email:** Envoyer les notifications par email en plus de l'interface
3. **Historique des statuts:** Tracker tous les changements de statut d'un CV
4. **Recherche de notifications:** Filtrer et rechercher dans les notifications
5. **Export du profil candidat:** Permettre l'export du profil en PDF
6. **Validation des liens:** Vérifier que les URLs GitHub/LinkedIn sont valides
7. **Tags de compétences:** Système de tags pour les compétences au lieu de texte libre

---

## 📝 9. NOTES IMPORTANTES

1. **Migration requise:** Exécuter le script SQL avant d'utiliser les nouvelles fonctionnalités
2. **Messages en français:** Toutes les notifications sont en français avec les accents corrects
3. **Sécurité:** Les boutons d'action utilisent des tokens AntiForgery
4. **Confirmation:** Les actions Accepter/Refuser demandent une confirmation utilisateur
5. **Statuts:** Les statuts valides sont: "En attente", "Accepte", "Refuse"

---

## 🔗 10. FICHIERS MODIFIÉS

### Backend (C#)
- ✅ `Candidat/Models/Utilisateur.cs`
- ✅ `Administration/Models/Utilisateur.cs`
- ✅ `Candidat/Models/ViewModels/ProfilePageViewModel.cs`
- ✅ `Candidat/Controllers/ProfileController.cs`

### Frontend (Razor)
- ✅ `Candidat/Views/Profile/Index.cshtml`

### Scripts
- ✅ `Administration/Scripts/AddCandidateProfileFields.sql`
- ✅ `IMPLEMENTATION_SUMMARY.md` (ce fichier)

---

## 📞 SUPPORT

Pour toute question ou problème concernant cette implémentation, veuillez consulter:
- Les controllers mentionnés ci-dessus pour la logique métier
- Les vues pour l'interface utilisateur
- Le script SQL pour la structure de la base de données

---

**Date d'implémentation:** 23 avril 2026  
**Version:** 1.0.0  
**Statut:** ✅ Implémenté et prêt pour les tests
