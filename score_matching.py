"""
================================================================================
    SCORE MATCHING - Système de Score de Correspondance Candidat/Offre
================================================================================

Auteur  : CvParsing Team
Version : 1.0.0
Date    : Avril 2026

Description :
    Module Python indépendant pour calculer les scores de correspondance
    entre un candidat et une offre d'emploi.

    Calcul basé sur :
        - Score diplôme
        - Score expérience
        - Score compétences
        - Bonus intelligents (diplôme supérieur, stages, frais diplômé)

    Ce fichier est INDÉPENDANT du projet principal.
    Il retourne uniquement des résultats exploitables par l'application.

================================================================================
"""

import json
from typing import Any, Optional

import pandas as pd


# ============================================================================
#  CONSTANTES ET CONFIGURATION
# ============================================================================

# Poids des scores dans le score global (modifiable dynamiquement)
POIDS_SCORE = {
    "diplome": 1.0,
    "experience": 1.0,
    "competence": 1.0,
}

# Barème des scores maximaux
SCORE_MAX = {
    "diplome": 30,
    "experience": 30,
    "competence": 40,
}

# Hiérarchie des diplômes (du plus bas au plus élevé)
HIERARCHIE_DIPLOMES = [
    "Sans diplôme",
    "Bac",
    "Bac+1",
    "Bac+2",
    "BTS",
    "DUT",
    "Bac+3",
    "Licence",
    "Bac+4",
    "Master",
    "Bac+5",
    "Ingénieur",
    "Doctorat",
]

# Bonus pour diplôme supérieur au diplôme requis
BONUS_DIPLOME_SUPERIEUR = {
    "Licence": {"Master": 1, "Doctorat": 2, "Ingénieur": 3},
    "Bac+3": {"Master": 1, "Doctorat": 2, "Ingénieur": 3},
    "Master": {"Doctorat": 2},
    "Bac+5": {"Doctorat": 2},
}

# Bonus stages et frais diplômé
BONUS_STAGE = 1        # +1 par stage
BONUS_FRAIS_DIPLOME = 1  # +1 si frais diplômé (dernier diplôme < 1 an)


# ============================================================================
#  DONNÉES MOCK (pour démonstration et tests)
# ============================================================================

CANDIDATS_MOCK = [
    {
        "id": 1,
        "nom": "Ahmed Benali",
        "email": "ahmed.benali@email.com",
        "diplome": "Master",
        "experience_annees": 3,
        "stages": 2,
        "frais_diplome": False,
        "competences": ["Python", "SQL", "Django", "React", "Git"],
    },
    {
        "id": 2,
        "nom": "Sara Khelifi",
        "email": "sara.khelifi@email.com",
        "diplome": "Licence",
        "experience_annees": 0,
        "stages": 1,
        "frais_diplome": True,
        "competences": ["Java", "Spring Boot", "SQL", "Angular"],
    },
    {
        "id": 3,
        "nom": "Youssef Amrani",
        "email": "youssef.amrani@email.com",
        "diplome": "Ingénieur",
        "experience_annees": 5,
        "stages": 0,
        "frais_diplome": False,
        "competences": ["Python", "Django", "Docker", "AWS", "SQL", "Git", "React"],
    },
    {
        "id": 4,
        "nom": "Fatima Zahra",
        "email": "fatima.zahra@email.com",
        "diplome": "Doctorat",
        "experience_annees": 2,
        "stages": 3,
        "frais_diplome": False,
        "competences": ["Python", "Machine Learning", "TensorFlow", "SQL", "R"],
    },
]

OFFRES_MOCK = [
    {
        "id": 1,
        "titre": "Développeur Python Senior",
        "departement": "Développement",
        "diplome_requis": "Licence",
        "experience_min": 2,
        "competences_requises": ["Python", "Django", "SQL", "Git"],
    },
    {
        "id": 2,
        "titre": "Développeur Full Stack",
        "departement": "Développement",
        "diplome_requis": "Master",
        "experience_min": 3,
        "competences_requises": ["Java", "Spring Boot", "Angular", "SQL", "Docker"],
    },
    {
        "id": 3,
        "titre": "Data Scientist",
        "departement": "Data",
        "diplome_requis": "Master",
        "experience_min": 1,
        "competences_requises": ["Python", "Machine Learning", "SQL", "TensorFlow"],
    },
]


# ============================================================================
#  CHARGEMENT DES DONNÉES
# ============================================================================


def charger_candidats(source: str = "mock") -> pd.DataFrame:
    """
    Charge les données des candidats depuis une source.

    Paramètres :
        source (str) : Source des données.
            - "mock" : Utilise les données de démonstration intégrées.
            - Chemin vers un fichier JSON ou CSV.

    Retourne :
        pd.DataFrame : DataFrame contenant les données des candidats.

    Exemple :
        >>> df = charger_candidats("mock")
        >>> df = charger_candidats("candidats.json")
        >>> df = charger_candidats("candidats.csv")
    """
    if source == "mock":
        return pd.DataFrame(CANDIDATS_MOCK)

    if source.endswith(".json"):
        with open(source, "r", encoding="utf-8") as f:
            data = json.load(f)
        return pd.DataFrame(data)

    if source.endswith(".csv"):
        return pd.read_csv(source, encoding="utf-8")

    raise ValueError(f"Format de source non supporté : {source}. Utilisez 'mock', '.json' ou '.csv'.")


def charger_offres(source: str = "mock") -> pd.DataFrame:
    """
    Charge les données des offres d'emploi depuis une source.

    Paramètres :
        source (str) : Source des données.
            - "mock" : Utilise les données de démonstration intégrées.
            - Chemin vers un fichier JSON ou CSV.

    Retourne :
        pd.DataFrame : DataFrame contenant les données des offres.

    Exemple :
        >>> df = charger_offres("mock")
        >>> df = charger_offres("offres.json")
    """
    if source == "mock":
        return pd.DataFrame(OFFRES_MOCK)

    if source.endswith(".json"):
        with open(source, "r", encoding="utf-8") as f:
            data = json.load(f)
        return pd.DataFrame(data)

    if source.endswith(".csv"):
        return pd.read_csv(source, encoding="utf-8")

    raise ValueError(f"Format de source non supporté : {source}. Utilisez 'mock', '.json' ou '.csv'.")


# ============================================================================
#  CALCUL DU SCORE DIPLÔME
# ============================================================================


def calculate_diploma_score(
    diplome_candidat: str,
    diplome_requis: str,
) -> tuple[float, float]:
    """
    Calcule le score de correspondance du diplôme.

    La logique compare le diplôme du candidat avec le diplôme requis
    par l'offre. Un score partiel est attribué si le diplôme du candidat
    est inférieur, et un score complet s'il est égal ou supérieur.

    Paramètres :
        diplome_candidat (str) : Diplôme du candidat (ex: "Master").
        diplome_requis (str)   : Diplôme requis par l'offre (ex: "Licence").

    Retourne :
        tuple[float, float] : (score_diplome, bonus_diplome)
            - score_diplome : Score entre 0 et SCORE_MAX["diplome"]
            - bonus_diplome : Bonus si diplôme supérieur au requis

    Exemple :
        >>> calculate_diploma_score("Master", "Licence")
        (30, 1)
        >>> calculate_diploma_score("Bac", "Licence")
        (10, 0)
    """
    score_max = SCORE_MAX["diplome"]

    # Récupérer les indices dans la hiérarchie
    try:
        idx_candidat = HIERARCHIE_DIPLOMES.index(diplome_candidat)
    except ValueError:
        idx_candidat = 0  # Diplôme non reconnu = "Sans diplôme"

    try:
        idx_requis = HIERARCHIE_DIPLOMES.index(diplome_requis)
    except ValueError:
        idx_requis = 0

    # --- Calcul du score de base ---
    if idx_candidat >= idx_requis:
        # Diplôme du candidat >= diplôme requis → score maximal
        score_diplome = score_max
    elif idx_candidat == 0:
        # Pas de diplôme → score 0
        score_diplome = 0
    else:
        # Diplôme inférieur → score proportionnel
        ratio = idx_candidat / idx_requis
        score_diplome = round(score_max * ratio * 0.7, 2)  # Pénalité de 30%

    # --- Calcul du bonus diplôme supérieur ---
    bonus_diplome = 0.0
    if diplome_requis in BONUS_DIPLOME_SUPERIEUR:
        bonus_table = BONUS_DIPLOME_SUPERIEUR[diplome_requis]
        bonus_diplome = bonus_table.get(diplome_candidat, 0)

    return score_diplome, bonus_diplome


# ============================================================================
#  CALCUL DU SCORE EXPÉRIENCE
# ============================================================================


def calculate_experience_score(
    experience_annees: float,
    experience_min_requise: float,
    stages: int = 0,
    frais_diplome: bool = False,
) -> tuple[float, float]:
    """
    Calcule le score de correspondance de l'expérience.

    Évalue l'expérience professionnelle du candidat par rapport
    au minimum requis par l'offre. Les stages et le statut de
    frais diplômé ajoutent des bonus.

    Paramètres :
        experience_annees      (float) : Années d'expérience du candidat.
        experience_min_requise (float) : Années d'expérience minimum requises.
        stages                 (int)   : Nombre de stages effectués.
        frais_diplome          (bool)  : True si le candidat est frais diplômé.

    Retourne :
        tuple[float, float] : (score_experience, bonus_experience)
            - score_experience : Score entre 0 et SCORE_MAX["experience"]
            - bonus_experience : Bonus stages + frais diplômé

    Exemple :
        >>> calculate_experience_score(3, 2, stages=2, frais_diplome=False)
        (30, 2)
        >>> calculate_experience_score(0, 1, stages=1, frais_diplome=True)
        (0, 2)
    """
    score_max = SCORE_MAX["experience"]

    # --- Score de base selon l'expérience ---
    if experience_annees >= experience_min_requise:
        # Expérience suffisante → score maximal
        score_experience = score_max
    elif experience_min_requise == 0:
        # Aucune expérience requise → score maximal
        score_experience = score_max
    elif experience_annees == 0:
        # Aucune expérience → score 0
        score_experience = 0
    else:
        # Expérience partielle → score proportionnel
        ratio = experience_annees / experience_min_requise
        score_experience = round(score_max * ratio * 0.8, 2)  # Pénalité de 20%

    # --- Bonus stages ---
    bonus_stages = stages * BONUS_STAGE

    # --- Bonus frais diplômé ---
    bonus_frais = BONUS_FRAIS_DIPLOME if frais_diplome else 0

    bonus_experience = bonus_stages + bonus_frais

    return score_experience, bonus_experience


# ============================================================================
#  CALCUL DU SCORE COMPÉTENCES
# ============================================================================


def calculate_skill_score(
    competences_candidat: list[str],
    competences_requises: list[str],
) -> tuple[float, float]:
    """
    Calcule le score de correspondance des compétences.

    Compare les compétences du candidat avec celles requises par l'offre.
    Le score est proportionnel au nombre de compétences correspondantes.

    Paramètres :
        competences_candidat  (list[str]) : Compétences du candidat.
        competences_requises  (list[str]) : Compétences requises par l'offre.

    Retourne :
        tuple[float, float] : (score_competence, pourcentage_matching)
            - score_competence : Score entre 0 et SCORE_MAX["competence"]
            - pourcentage_matching : Pourcentage de correspondance (0-100)

    Exemple :
        >>> calculate_skill_score(
        ...     ["Python", "SQL", "Django", "React"],
        ...     ["Python", "Django", "SQL", "Git"]
        ... )
        (30.0, 75.0)
    """
    score_max = SCORE_MAX["competence"]

    if not competences_requises:
        # Aucune compétence requise → score maximal
        return score_max, 100.0

    if not competences_candidat:
        # Aucune compétence chez le candidat → score 0
        return 0, 0.0

    # Normaliser les compétences en minuscules pour la comparaison
    cand_norm = [c.lower().strip() for c in competences_candidat]
    req_norm = [r.lower().strip() for r in competences_requises]

    # Identifier les compétences correspondantes
    competences_matchees = [c for c in req_norm if c in cand_norm]

    # Calcul du pourcentage de matching
    total_requises = len(req_norm)
    total_matchees = len(competences_matchees)
    pourcentage_matching = round((total_matchees / total_requises) * 100, 2) if total_requises > 0 else 0

    # Calcul du score de compétence
    score_competence = round(score_max * (total_matchees / total_requises), 2) if total_requises > 0 else 0

    return score_competence, pourcentage_matching


# ============================================================================
#  CALCUL DU BONUS GLOBAL
# ============================================================================


def calculate_bonus(
    diplome_candidat: str,
    diplome_requis: str,
    stages: int = 0,
    frais_diplome: bool = False,
) -> float:
    """
    Calcule le bonus total applicable au candidat.

    Le bonus combine :
        - Bonus diplôme supérieur (si le diplôme du candidat dépasse le requis)
        - Bonus stages (+1 par stage)
        - Bonus frais diplômé (+1 si récemment diplômé)

    Paramètres :
        diplome_candidat (str)  : Diplôme du candidat.
        diplome_requis   (str)  : Diplôme requis par l'offre.
        stages           (int)  : Nombre de stages effectués.
        frais_diplome    (bool) : True si le candidat est frais diplômé.

    Retourne :
        float : Bonus total accumulé.

    Exemple :
        >>> calculate_bonus("Ingénieur", "Licence", stages=2, frais_diplome=False)
        5.0
    """
    # Bonus diplôme supérieur
    _, bonus_diplome = calculate_diploma_score(diplome_candidat, diplome_requis)

    # Bonus stages
    bonus_stages = stages * BONUS_STAGE

    # Bonus frais diplômé
    bonus_frais = BONUS_FRAIS_DIPLOME if frais_diplome else 0

    return bonus_diplome + bonus_stages + bonus_frais


# ============================================================================
#  CALCUL DU SCORE GLOBAL
# ============================================================================


def calculate_global_score(
    candidat: dict[str, Any] | pd.Series,
    offre: dict[str, Any] | pd.Series,
) -> dict[str, Any]:
    """
    Calcule le score global de correspondance entre un candidat et une offre.

    Combine les scores de diplôme, expérience et compétences,
    puis ajoute les bonus intelligents pour produire un score final.

    Paramètres :
        candidat (dict | pd.Series) : Données du candidat.
        offre    (dict | pd.Series) : Données de l'offre d'emploi.

    Retourne :
        dict : Résultat structuré contenant tous les scores et détails.

    Structure du résultat :
        {
            "candidat_id": int,
            "candidat_nom": str,
            "offre_id": int,
            "offre_titre": str,
            "score_diplome": float,
            "score_experience": float,
            "score_competence": float,
            "bonus": float,
            "score_global": float,
            "matching_percentage": float,
            "competences_matchees": list[str],
            "competences_manquantes": list[str],
            "details": {
                "bonus_diplome_superieur": float,
                "bonus_stages": float,
                "bonus_frais_diplome": float,
            }
        }

    Exemple :
        >>> resultat = calculate_global_score(candidat_dict, offre_dict)
        >>> print(resultat["score_global"])
        85.0
    """
    # Normaliser les données d'entrée (dict ou Series → dict)
    c = dict(candidat) if isinstance(candidat, pd.Series) else candidat
    o = dict(offre) if isinstance(offre, pd.Series) else offre

    # --- Extraire les données du candidat ---
    diplome_candidat = c.get("diplome", "Sans diplôme")
    experience_annees = float(c.get("experience_annees", 0))
    stages = int(c.get("stages", 0))
    frais_diplome = bool(c.get("frais_diplome", False))
    competences_candidat = c.get("competences", [])
    if isinstance(competences_candidat, str):
        # Si les compétences sont une chaîne, les séparer par virgule
        competences_candidat = [s.strip() for s in competences_candidat.split(",") if s.strip()]

    # --- Extraire les données de l'offre ---
    diplome_requis = o.get("diplome_requis", "Sans diplôme")
    experience_min = float(o.get("experience_min", 0))
    competences_requises = o.get("competences_requises", [])
    if isinstance(competences_requises, str):
        competences_requises = [s.strip() for s in competences_requises.split(",") if s.strip()]

    # --- Calcul des scores individuels ---
    score_diplome, bonus_diplome = calculate_diploma_score(diplome_candidat, diplome_requis)

    score_experience, bonus_experience = calculate_experience_score(
        experience_annees, experience_min, stages, frais_diplome
    )

    score_competence, matching_percentage = calculate_skill_score(
        competences_candidat, competences_requises
    )

    # --- Calcul du bonus total ---
    bonus_stages = stages * BONUS_STAGE
    bonus_frais = BONUS_FRAIS_DIPLOME if frais_diplome else 0
    bonus_total = bonus_diplome + bonus_stages + bonus_frais

    # --- Calcul du score global ---
    score_global = (
        score_diplome * POIDS_SCORE["diplome"]
        + score_experience * POIDS_SCORE["experience"]
        + score_competence * POIDS_SCORE["competence"]
        + bonus_total
    )

    # --- Compétences matchées et manquantes ---
    cand_norm = [c.lower().strip() for c in competences_candidat]
    req_norm = [r.lower().strip() for r in competences_requises]
    competences_matchees = [r for r in competences_requises if r.lower().strip() in cand_norm]
    competences_manquantes = [r for r in competences_requises if r.lower().strip() not in cand_norm]

    # --- Construire le résultat ---
    resultat = {
        "candidat_id": c.get("id", 0),
        "candidat_nom": c.get("nom", c.get("NomUtilisateur", "Inconnu")),
        "offre_id": o.get("id", 0),
        "offre_titre": o.get("titre", o.get("Titre", "Offre inconnue")),
        "score_diplome": score_diplome,
        "score_experience": score_experience,
        "score_competence": score_competence,
        "bonus": bonus_total,
        "score_global": round(score_global, 2),
        "matching_percentage": matching_percentage,
        "competences_matchees": competences_matchees,
        "competences_manquantes": competences_manquantes,
        "details": {
            "bonus_diplome_superieur": bonus_diplome,
            "bonus_stages": bonus_stages,
            "bonus_frais_diplome": bonus_frais,
        },
    }

    return resultat


# ============================================================================
#  MATCHING MULTIPLE : Comparer un candidat à plusieurs offres
# ============================================================================


def matching_candidat_offres(
    candidat: dict[str, Any] | pd.Series,
    offres: pd.DataFrame,
) -> pd.DataFrame:
    """
    Compare un candidat à toutes les offres disponibles.

    Paramètres :
        candidat (dict | pd.Series) : Données du candidat.
        offres   (pd.DataFrame)     : DataFrame des offres d'emploi.

    Retourne :
        pd.DataFrame : Résultats triés par score_global décroissant.
    """
    resultats = []
    for _, offre in offres.iterrows():
        resultat = calculate_global_score(candidat, offre)
        resultats.append(resultat)

    df_resultats = pd.DataFrame(resultats)
    df_resultats = df_resultats.sort_values(by="score_global", ascending=False).reset_index(drop=True)

    return df_resultats


# ============================================================================
#  MATCHING MULTIPLE : Comparer plusieurs candidats à une offre
# ============================================================================


def matching_offre_candidats(
    offre: dict[str, Any] | pd.Series,
    candidats: pd.DataFrame,
) -> pd.DataFrame:
    """
    Compare tous les candidats à une offre spécifique.

    Paramètres :
        offre     (dict | pd.Series) : Données de l'offre d'emploi.
        candidats (pd.DataFrame)     : DataFrame des candidats.

    Retourne :
        pd.DataFrame : Résultats triés par score_global décroissant.
    """
    resultats = []
    for _, candidat in candidats.iterrows():
        resultat = calculate_global_score(candidat, offre)
        resultats.append(resultat)

    df_resultats = pd.DataFrame(resultats)
    df_resultats = df_resultats.sort_values(by="score_global", ascending=False).reset_index(drop=True)

    return df_resultats


# ============================================================================
#  INTÉGRATION IA (Placeholder modulaire)
# ============================================================================


def calculate_ai_score(
    candidat: dict[str, Any],
    offre: dict[str, Any],
    api_key: Optional[str] = None,
    model: str = "llama3-70b-8192",
) -> dict[str, Any]:
    """
    Placeholder pour l'intégration d'un modèle IA (ex: Groq API, NLP).

    Cette fonction est un point d'extension pour intégrer un modèle
    d'intelligence artificielle qui pourrait :
        - Analyser sémantiquement les compétences
        - Évaluer la correspondance via NLP
        - Affiner les scores avec un modèle entraîné

    ⚠️ Ne jamais coder en dur les clés API.
    ⚠️ Passer les clés via paramètre ou variable d'environnement.

    Paramètres :
        candidat (dict)       : Données du candidat.
        offre    (dict)       : Données de l'offre.
        api_key  (str|None)   : Clé API (optionnelle, peut venir d'env var).
        model    (str)        : Nom du modèle IA à utiliser.

    Retourne :
        dict : Résultat IA structuré (placeholder pour l'instant).

    Exemple d'intégration future :
        >>> import os
        >>> api_key = os.environ.get("GROQ_API_KEY")
        >>> resultat_ia = calculate_ai_score(candidat, offre, api_key=api_key)
    """
    # --- Placeholder : retourner un résultat par défaut ---
    # Pour intégrer un modèle IA, décommenter et adapter :

    # import os
    # import requests
    #
    # if api_key is None:
    #     api_key = os.environ.get("GROQ_API_KEY")
    #
    # if api_key is None:
    #     return {"ai_score": None, "ai_confidence": None, "error": "Clé API non fournie"}
    #
    # prompt = f"""
    #     Évalue la correspondance entre ce candidat et cette offre :
    #     Candidat : {json.dumps(candidat, ensure_ascii=False)}
    #     Offre : {json.dumps(offre, ensure_ascii=False)}
    #     Retourne un score de 0 à 100 et un niveau de confiance.
    # """
    #
    # response = requests.post(
    #     "https://api.groq.com/openai/v1/chat/completions",
    #     headers={"Authorization": f"Bearer {api_key}"},
    #     json={
    #         "model": model,
    #         "messages": [{"role": "user", "content": prompt}],
    #         "temperature": 0.3,
    #     },
    #     timeout=30,
    # )
    # ... parser la réponse et retourner le score IA

    return {
        "ai_score": None,
        "ai_confidence": None,
        "ai_model": model,
        "status": "placeholder - non intégré",
        "message": "Intégrez votre modèle IA en décommentant le code dans cette fonction.",
    }


# ============================================================================
#  AFFICHAGE PROFESSIONNEL
# ============================================================================


def afficher_resultat(resultat: dict[str, Any]) -> None:
    """
    Affiche un résultat de matching de manière professionnelle dans la console.

    Paramètres :
        resultat (dict) : Résultat retourné par calculate_global_score().
    """
    print("\n" + "=" * 60)
    print("  RÉSULTAT DU MATCHING CANDIDAT / OFFRE")
    print("=" * 60)

    print(f"\n  Candidat   : {resultat['candidat_nom']}")
    print(f"  Offre      : {resultat['offre_titre']}")

    print("\n  ┌─────────────────────────────────────────────┐")
    print(f"  │  Score Diplôme      :  {resultat['score_diplome']:>6.1f} / {SCORE_MAX['diplome']:<4}      │")
    print(f"  │  Score Expérience   :  {resultat['score_experience']:>6.1f} / {SCORE_MAX['experience']:<4}      │")
    print(f"  │  Score Compétences  :  {resultat['score_competence']:>6.1f} / {SCORE_MAX['competence']:<4}      │")
    print(f"  │  Bonus              :  {resultat['bonus']:>6.1f}            │")
    print("  ├─────────────────────────────────────────────┤")
    print(f"  │  ★ SCORE GLOBAL     :  {resultat['score_global']:>6.1f}            │")
    print(f"  │  Matching           :  {resultat['matching_percentage']:>5.1f}%             │")
    print("  └─────────────────────────────────────────────┘")

    # Compétences matchées
    if resultat.get("competences_matchees"):
        print(f"\n  ✅ Compétences matchées    : {', '.join(resultat['competences_matchees'])}")

    # Compétences manquantes
    if resultat.get("competences_manquantes"):
        print(f"  ❌ Compétences manquantes  : {', '.join(resultat['competences_manquantes'])}")

    # Détails bonus
    details = resultat.get("details", {})
    print(f"\n  📊 Détails bonus :")
    print(f"     - Diplôme supérieur : +{details.get('bonus_diplome_superieur', 0)}")
    print(f"     - Stages            : +{details.get('bonus_stages', 0)}")
    print(f"     - Frais diplômé     : +{details.get('bonus_frais_diplome', 0)}")

    print("\n" + "=" * 60 + "\n")


def afficher_classement(df_resultats: pd.DataFrame, titre: str = "Classement") -> None:
    """
    Affiche un classement de résultats sous forme de tableau professionnel.

    Paramètres :
        df_resultats (pd.DataFrame) : DataFrame des résultats de matching.
        titre        (str)          : Titre du classement.
    """
    print(f"\n{'=' * 70}")
    print(f"  {titre.upper()}")
    print(f"{'=' * 70}\n")

    # Formater l'affichage
    display_cols = ["candidat_nom", "offre_titre", "score_global", "matching_percentage"]
    available_cols = [c for c in display_cols if c in df_resultats.columns]

    if available_cols:
        df_display = df_resultats[available_cols].copy()
        df_display.columns = ["Candidat", "Offre", "Score Global", "Matching %"]

        # Ajouter le rang
        df_display.insert(0, "#", range(1, len(df_display) + 1))

        print(df_display.to_string(index=False))
    else:
        print(df_resultats.to_string(index=False))

    print(f"\n{'=' * 70}\n")


# ============================================================================
#  EXPORT DES RÉSULTATS
# ============================================================================


def exporter_resultats_json(resultats: list[dict] | pd.DataFrame, chemin: str) -> None:
    """
    Exporte les résultats au format JSON.

    Paramètres :
        resultats (list[dict] | pd.DataFrame) : Résultats à exporter.
        chemin    (str)                        : Chemin du fichier de sortie.
    """
    if isinstance(resultats, pd.DataFrame):
        data = resultats.to_dict(orient="records")
    else:
        data = resultats

    with open(chemin, "w", encoding="utf-8") as f:
        json.dump(data, f, ensure_ascii=False, indent=2)

    print(f"✅ Résultats exportés vers : {chemin}")


def exporter_resultats_csv(df_resultats: pd.DataFrame, chemin: str) -> None:
    """
    Exporte les résultats au format CSV.

    Paramètres :
        df_resultats (pd.DataFrame) : DataFrame des résultats.
        chemin       (str)          : Chemin du fichier de sortie.
    """
    df_resultats.to_csv(chemin, index=False, encoding="utf-8")
    print(f"✅ Résultats exportés vers : {chemin}")


# ============================================================================
#  POINT D'ENTRÉE PRINCIPAL (Démonstration)
# ============================================================================


def main() -> None:
    """
    Fonction principale de démonstration.

    Charge les données mock, effectue les calculs de matching,
    affiche les résultats et exporte en JSON.
    """
    print("\n" + "🔹" * 30)
    print("  SCORE MATCHING - Système de Correspondance")
    print("🔹" * 30)

    # --- Charger les données ---
    df_candidats = charger_candidats("mock")
    df_offres = charger_offres("mock")

    print(f"\n📂 {len(df_candidats)} candidat(s) chargé(s)")
    print(f"📂 {len(df_offres)} offre(s) chargée(s)")

    # --- Exemple 1 : Un candidat vs une offre ---
    print("\n" + "─" * 50)
    print("  EXEMPLE 1 : Ahmed Benali → Développeur Python Senior")
    print("─" * 50)

    candidat_1 = df_candidats.iloc[0]
    offre_1 = df_offres.iloc[0]
    resultat_1 = calculate_global_score(candidat_1, offre_1)
    afficher_resultat(resultat_1)

    # --- Exemple 2 : Un candidat vs toutes les offres ---
    print("─" * 50)
    print("  EXEMPLE 2 : Youssef Amrani → Toutes les offres")
    print("─" * 50)

    candidat_3 = df_candidats.iloc[2]
    resultats_candidat = matching_candidat_offres(candidat_3, df_offres)
    afficher_classement(resultats_candidat, titre="Youssef Amrani - Offres classées")

    # --- Exemple 3 : Tous les candidats vs une offre ---
    print("─" * 50)
    print("  EXEMPLE 3 : Tous les candidats → Développeur Python Senior")
    print("─" * 50)

    offre_python = df_offres.iloc[0]
    resultats_offre = matching_offre_candidats(offre_python, df_candidats)
    afficher_classement(resultats_offre, titre="Développeur Python Senior - Candidats classés")

    # --- Exemple 4 : Tous les candidats vs toutes les offres ---
    print("─" * 50)
    print("  EXEMPLE 4 : Matching complet")
    print("─" * 50)

    tous_resultats = []
    for _, offre in df_offres.iterrows():
        for _, candidat in df_candidats.iterrows():
            resultat = calculate_global_score(candidat, offre)
            tous_resultats.append(resultat)

    df_tous = pd.DataFrame(tous_resultats)
    df_tous = df_tous.sort_values(by="score_global", ascending=False).reset_index(drop=True)
    afficher_classement(df_tous, titre="Matching Complet - Tous les résultats")

    # --- Export JSON ---
    exporter_resultats_json(df_tous, "resultats_matching.json")

    # --- Test IA placeholder ---
    print("─" * 50)
    print("  Test IA (placeholder)")
    print("─" * 50)
    resultat_ia = calculate_ai_score(dict(candidat_1), dict(offre_1))
    print(f"  Statut IA : {resultat_ia['status']}")
    print(f"  Message   : {resultat_ia['message']}")

    print("\n✅ Démonstration terminée avec succès !\n")


# ============================================================================
#  EXÉCUTION
# ============================================================================

if __name__ == "__main__":
    main()
