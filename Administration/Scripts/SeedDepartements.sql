-- Seed script for departments based on the organizational chart
-- Run this after creating the Departement table

INSERT INTO Departement (Nom, Description, DateCreation, IsActive) VALUES
('Structure visites de risques', 'Structure des visites de risques', GETDATE(), 1),
('Département Santé et Prévoyance Collective', 'Département Santé et Prévoyance Collective', GETDATE(), 1),
('Département Vie et Capitalisation', 'Département Vie et Capitalisation', GETDATE(), 1),
('Département Réassurance', 'Département Réassurance', GETDATE(), 1),
('Direction Automobile et Assurances des Particuliers', 'Direction Automobile et Assurances des Particuliers', GETDATE(), 1),
('Direction Assurances d''Entreprises', 'Direction Assurances d''Entreprises', GETDATE(), 1),
('Direction des Affaires Administratives et GRH', 'Direction des Affaires Administratives et GRH', GETDATE(), 1),
('Direction Financière', 'Direction Financière', GETDATE(), 1),
('Direction Comptabilité et Contrôle de Gestion', 'Direction Comptabilité et Contrôle de Gestion', GETDATE(), 1),
('Direction Commerciale', 'Direction Commerciale', GETDATE(), 1),
('Direction des Systèmes d''Information', 'Direction des Systèmes d''Information', GETDATE(), 1),
('Direction Audit Stratégie et Risques', 'Direction Audit Stratégie et Risques', GETDATE(), 1);

-- Service Gestion sinistres Maladie
-- Service Technique Vie
-- Service Gestion Technique Réassurance
-- Service Commercial Vie
-- Service Etablissement des Comptes Réassurance
-- Service Assurances Crédit
-- Département Production Auto et Risques des Particuliers
-- Département sinistres Auto
-- Département Risques Agricoles
-- Département Transport
-- Département Risques Techniques
-- Département IARD Entreprises
-- Département Indemnisation IARDS
-- Service Gestion des Ressources Humaines
-- Service Formation
-- Service Bureau d''Ordre et Economat
-- Service Administratif et Patrimoine immobilier
-- Service Recouvrement
-- Service Financier et Placement
-- Centrale des Chèques
-- Service Inspection
-- Service Juridique
-- Service Reporting et Arrêté des comptes
-- Section Comptabilité Agences
-- Service Fiscalité et Rapprochement
-- Service Trésorerie et Comptabilité Tiers
-- Service Comptabilité Analytique et Immobilisations
-- Département Marketing & développement des produits
-- Département Animation commerciale et gestion des offres
-- Agence Centrale
-- Service Assistance Aux Voyages
-- Département Production
-- Département Infrastructure
-- Département Études et Développement
-- Département Stratégie
-- Département Gestion de Risques et Actuariat
-- Département Audit interne
-- Département Conformité
