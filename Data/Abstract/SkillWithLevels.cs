using Systems.SimpleCore.Storage.Lists;
using Unity.Mathematics;

namespace Systems.SimpleSkills.Data.Abstract
{
    /// <summary>
    ///     Base class for skills that have multiple level variants.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Prerequisites and costs for leveling up should be implemented externally via
    ///         <see cref="SkillBase.ConsumeResources"/> and <see cref="SkillBase.CheckAttemptSuccess"/> overrides.
    ///     </para>
    ///     <para>
    ///         <see cref="TSelfBaseSkill"/> should be an abstract class that is used as main skill
    ///         class for all skill variants (it contains base callbacks and validations if they're not
    ///         implemented in the skill variant).
    ///     </para>
    /// </remarks>
    public abstract class SkillWithLevels<TSelfBaseSkill> : SkillBase, ISkillWithLevels
        where TSelfBaseSkill : SkillWithLevels<TSelfBaseSkill>
    {
        /// <summary>
        ///     The level/rank of this specific skill variant.
        ///     Each asset in the database represents a specific level.
        /// </summary>
        public virtual int Level => 1;

        /// <summary>
        ///     Resolves the skill asset for a given level.
        /// </summary>
        /// <param name="level">The desired skill level (1-based)</param>
        /// <returns>
        ///     The skill asset matching the requested level, or the closest available level
        ///     if the exact level doesn't exist. Returns this skill if no variants are configured.
        /// </returns>
        public virtual SkillBase GetSkillForLevel(int level)
        {
            // Get skills from database
            ROListAccess<TSelfBaseSkill> sameSkills = SkillsDatabase.GetAll<TSelfBaseSkill>();
            
            TSelfBaseSkill bestFound = null;
            int distance = int.MaxValue;
            
            // Find the skill with the requested level
            for(int i = 0; i < sameSkills.List.Count; i++)
            {
                if (sameSkills.List[i].Level == level)
                {
                    TSelfBaseSkill result = sameSkills.List[i];
                    sameSkills.Release();
                    return result;
                }
                
                // Compute absolute distance to match best skill
                int newDistance = math.abs(sameSkills.List[i].Level - level);
                if (newDistance >= distance) continue;
                
                bestFound = sameSkills.List[i];
                distance = newDistance;
            }
           
            sameSkills.Release();
            return bestFound;
        }
    }
}
