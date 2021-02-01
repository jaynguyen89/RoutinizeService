<?php
declare(strict_types=1);

namespace App\Model\Entity;

use Cake\ORM\Entity;

/**
 * Token Entity
 *
 * @property int $TokenId
 * @property string $TokenString
 * @property \Cake\I18n\FrozenTime $TimeStamp
 * @property int|null $Life
 * @property string|null $Target
 */
class Token extends Entity
{
    /**
     * Fields that can be mass assigned using newEntity() or patchEntity().
     *
     * Note that when '*' is set to true, this allows all unspecified fields to
     * be mass assigned. For security purposes, it is advised to set '*' to false
     * (or remove it), and explicitly make individual fields accessible as needed.
     *
     * @var array
     */
    protected $_accessible = [
        'TokenString' => true,
        'TimeStamp' => true,
        'Life' => true,
        'Target' => true,
    ];
}
