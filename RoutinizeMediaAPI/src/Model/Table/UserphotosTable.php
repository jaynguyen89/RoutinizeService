<?php
declare(strict_types=1);

namespace App\Model\Table;

use Cake\ORM\Query;
use Cake\ORM\RulesChecker;
use Cake\ORM\Table;
use Cake\Validation\Validator;

/**
 * Userphotos Model
 *
 * @method \App\Model\Entity\Userphoto newEmptyEntity()
 * @method \App\Model\Entity\Userphoto newEntity(array $data, array $options = [])
 * @method \App\Model\Entity\Userphoto[] newEntities(array $data, array $options = [])
 * @method \App\Model\Entity\Userphoto get($primaryKey, $options = [])
 * @method \App\Model\Entity\Userphoto findOrCreate($search, ?callable $callback = null, $options = [])
 * @method \App\Model\Entity\Userphoto patchEntity(\Cake\Datasource\EntityInterface $entity, array $data, array $options = [])
 * @method \App\Model\Entity\Userphoto[] patchEntities(iterable $entities, array $data, array $options = [])
 * @method \App\Model\Entity\Userphoto|false save(\Cake\Datasource\EntityInterface $entity, $options = [])
 * @method \App\Model\Entity\Userphoto saveOrFail(\Cake\Datasource\EntityInterface $entity, $options = [])
 * @method \App\Model\Entity\Userphoto[]|\Cake\Datasource\ResultSetInterface|false saveMany(iterable $entities, $options = [])
 * @method \App\Model\Entity\Userphoto[]|\Cake\Datasource\ResultSetInterface saveManyOrFail(iterable $entities, $options = [])
 * @method \App\Model\Entity\Userphoto[]|\Cake\Datasource\ResultSetInterface|false deleteMany(iterable $entities, $options = [])
 * @method \App\Model\Entity\Userphoto[]|\Cake\Datasource\ResultSetInterface deleteManyOrFail(iterable $entities, $options = [])
 */
class UserphotosTable extends Table
{
    /**
     * Initialize method
     *
     * @param array $config The configuration for the Table.
     * @return void
     */
    public function initialize(array $config): void
    {
        parent::initialize($config);

        $this->setTable('userphotos');
        $this->setDisplayField('Id');
        $this->setPrimaryKey('Id');
    }

    /**
     * Default validation rules.
     *
     * @param \Cake\Validation\Validator $validator Validator instance.
     * @return \Cake\Validation\Validator
     */
    public function validationDefault(Validator $validator): Validator
    {
        $validator
            ->integer('Id')
            ->allowEmptyString('Id', null, 'create');

        $validator
            ->integer('PhotoId')
            ->allowEmptyString('PhotoId');

        $validator
            ->integer('HidrogenianId')
            ->allowEmptyString('HidrogenianId');

        $validator
            ->boolean('IsAvatar')
            ->allowEmptyString('IsAvatar');

        $validator
            ->boolean('IsCover')
            ->allowEmptyString('IsCover');

        return $validator;
    }
}
